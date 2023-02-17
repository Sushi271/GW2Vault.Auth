using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.DTOs.Entities;
using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Repositories;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class KeyPairService : BaseService, IKeyPairService
    {
        IKeyPairRepository KeyPairRepository { get; }
        IControllerActionKeyPairRepository ControllerActionKeyPairRepository { get; }
        IAccountMachineKeyPairRepository AccountMachineKeyPairRepository { get; }
        IAccountRepository AccountRepository { get; }

        public KeyPairService(AuthContext context,
            IKeyPairRepository keyPairRepository,
            IControllerActionKeyPairRepository controllerActionKeyPairRepository,
            IAccountMachineKeyPairRepository accountMachineKeyPairRepository,
            IAccountRepository accountRepository)
            : base(context)
        {
            KeyPairRepository = keyPairRepository;
            ControllerActionKeyPairRepository = controllerActionKeyPairRepository;
            AccountMachineKeyPairRepository = accountMachineKeyPairRepository;
            AccountRepository = accountRepository;
        }

        public ServiceResponse<List<SimplifiedKeyPairEntity>> GetKeyPairList()
        {
            var keyPairs = KeyPairRepository.GetList();
            var dtos = keyPairs.Select(kp =>
            {
                var caKeyPair = ControllerActionKeyPairRepository.GetByKeyPairId(kp.Id);
                if (caKeyPair != null)
                {
                    return new SimplifiedKeyPairEntity
                    {
                        Id = kp.Id,
                        Purpose = KeyPairPurpose.ControllerAction,
                        PurposeDetails = $"{caKeyPair.ControllerName}.{caKeyPair.ActionName}",
                        Type = caKeyPair.KeyPairTypeId
                    };
                }

                var amKeyPair = AccountMachineKeyPairRepository.GetByKeyPairId(kp.Id);
                if (amKeyPair != null)
                {
                    var boundAccount = AccountRepository.GetById(amKeyPair.AccountId);
                    return new SimplifiedKeyPairEntity
                    {
                        Id = kp.Id,
                        Purpose = KeyPairPurpose.AccountMachine,
                        PurposeDetails = $"{boundAccount.AccountName}[{amKeyPair.MachineId}]",
                        Type = amKeyPair.KeyPairTypeId
                    };
                }

                return new SimplifiedKeyPairEntity
                {
                    Id = kp.Id,
                    Purpose = KeyPairPurpose.Undefined
                };
            }).ToList();
            return ServiceResponse.Success(dtos);
        }

        public ServiceResponse<AbstractKeyPairEntity> GetKeyPair(int keyPairId) => TryExecute(() =>
        {
            var keyPair = KeyPairRepository.GetById(keyPairId);
            if (keyPair == null)
                return ServiceResponse.NotFound<AbstractKeyPairEntity>("There is no KeyPair with given Id.");

            var entity = GetKeyPairEntity(keyPairId);
            entity.Id = keyPair.Id;
            entity.PrivateKey = keyPair.PrivateKey;
            entity.PublicKey = keyPair.PublicKey;
            return ServiceResponse.Success(entity);
        });

        AbstractKeyPairEntity GetKeyPairEntity(int keyPairId)
        {
            // Yeah I could've done it all with factories but I'm lazy today
            var caKeyPair = ControllerActionKeyPairRepository.GetByKeyPairId(keyPairId);
            if (caKeyPair != null)
            {
                return new ControllerActionKeyPairEntity
                {
                    ControllerName = caKeyPair.ControllerName,
                    ActionName = caKeyPair.ActionName,
                    Type = caKeyPair.KeyPairTypeId
                };
            }

            var amKeyPair = AccountMachineKeyPairRepository.GetByKeyPairId(keyPairId);
            if (amKeyPair != null)
            {
                var boundAccount = AccountRepository.GetById(amKeyPair.AccountId);
                return new AccountMachineKeyPairEntity
                {
                    AccountId = amKeyPair.AccountId,
                    AccountName = boundAccount.AccountName,
                    MachineId = amKeyPair.MachineId,
                    Type = amKeyPair.KeyPairTypeId
                };
            }

            return new UndefinedKeyPairEntity();
        }

        public ServiceResponse<AbstractKeyPairEntity> SaveKeyPair(SaveKeyPairRequest keyPairRequest)
            => TryExecute(inTransaction: true, action: () =>
            {
                var rsa = GetKeyPairFromString(keyPairRequest.PrivateKey);
                var base64Public = ExtractBase64PublicKey(rsa);

                int keyPairId = keyPairRequest.Id ?? 0;
                KeyPair keyPair;
                if (keyPairRequest.Id == null)
                {
                    keyPair = new KeyPair
                    {
                        Id = keyPairId,
                        PrivateKey = keyPairRequest.PrivateKey,
                        PublicKey = base64Public
                    };
                    keyPair = KeyPairRepository.Insert(keyPair);
                    keyPairId = keyPair.Id;
                }
                else
                {
                    keyPair = KeyPairRepository.GetById(keyPairId);
                    if (keyPair == null)
                        return ServiceResponse.NotFound<AbstractKeyPairEntity>("There is no KeyPair with given Id.");
                }

                var caKeyPair = ControllerActionKeyPairRepository.GetByKeyPairId(keyPairId);
                var amKeyPair = AccountMachineKeyPairRepository.GetByKeyPairId(keyPairId);

                if (keyPairRequest.Purpose != KeyPairPurpose.ControllerAction && caKeyPair != null)
                    ControllerActionKeyPairRepository.Delete(caKeyPair);
                if (keyPairRequest.Purpose != KeyPairPurpose.AccountMachine && amKeyPair != null)
                    AccountMachineKeyPairRepository.Delete(amKeyPair);

                if (keyPairRequest.Purpose == KeyPairPurpose.ControllerAction)
                {
                    if (caKeyPair == null)
                        caKeyPair = new ControllerActionKeyPair { KeyPairId = keyPairId, };
                    caKeyPair.KeyPairTypeId = keyPairRequest.Type.Value;
                    caKeyPair.ControllerName = keyPairRequest.ControllerName;
                    caKeyPair.ActionName = keyPairRequest.ActionName;

                    if (caKeyPair == null)
                        ControllerActionKeyPairRepository.Insert(caKeyPair);
                    else ControllerActionKeyPairRepository.Update(caKeyPair);
                }

                if (keyPairRequest.Purpose == KeyPairPurpose.AccountMachine)
                {
                    if (amKeyPair == null)
                        amKeyPair = new AccountMachineKeyPair { KeyPairId = keyPairId, };
                    amKeyPair.KeyPairTypeId = keyPairRequest.Type.Value;
                    amKeyPair.AccountId = keyPairRequest.AccountId.Value;
                    amKeyPair.MachineId = keyPairRequest.MachineId.Value;

                    if (amKeyPair == null)
                        AccountMachineKeyPairRepository.Insert(amKeyPair);
                    else AccountMachineKeyPairRepository.Update(amKeyPair);
                }

                var entity = GetKeyPairEntity(keyPairId);
                entity.Id = keyPairId;
                entity.PrivateKey = keyPair.PrivateKey;
                entity.PublicKey = keyPair.PublicKey;
                return ServiceResponse.Success(entity);
            });

        static AsymmetricCipherKeyPair GetKeyPairFromString(string privateKeyString)
        {
            using var stream = new MemoryStream();
            using var sw = new StreamWriter(stream);
            sw.Write(privateKeyString);
            sw.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            using var sr = new StreamReader(stream);
            var pr = new PemReader(sr);
            var keyPair = (AsymmetricCipherKeyPair)pr.ReadObject();

            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);

            return keyPair;
        }

        static string ExtractBase64PublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var bytes = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public).GetDerEncoded();
            return Convert.ToBase64String(bytes);
        }

        public ServiceResponse RemoveKeyPair(int keyPairId)
            => TryExecute(inTransaction: true, action: () =>
            {
                var keyPair = KeyPairRepository.GetById(keyPairId);
                if (keyPair == null)
                    return ServiceResponse.NotFound<AbstractKeyPairEntity>("There is no KeyPair with given Id.");

                var caKeyPair = ControllerActionKeyPairRepository.GetByKeyPairId(keyPairId);
                if (caKeyPair != null)
                    ControllerActionKeyPairRepository.Delete(caKeyPair);

                var amKeyPair = AccountMachineKeyPairRepository.GetByKeyPairId(keyPairId);
                if (amKeyPair != null)
                    AccountMachineKeyPairRepository.Delete(amKeyPair);

                KeyPairRepository.Delete(keyPair);

                return ServiceResponse.Success();
            });
    }
}
