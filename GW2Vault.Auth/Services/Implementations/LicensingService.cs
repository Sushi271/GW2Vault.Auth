using System;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Repositories;
using Microsoft.AspNetCore.Http;
using GW2Vault.Auth.DTOs.Requests;
using System.Text;
using System.Security.Cryptography;
using GW2Vault.Auth.DTOs.Responses;
using System.Collections.Generic;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class LicensingService : BaseService, ILicensingService
    {
        IEncryptionService EncryptionService { get; }
        IAccountRepository AccountRepository { get; }
        IMachineRepository MachineRepository { get; }
        IUniqueMinerRepository UniqueMinerRepository { get; }
        IActivationCodeRepository ActivationCodeRepository { get; }
        IMachineActivationRepository MachineActivationRepository { get; }
        IActivationRequestLogRepository ActivationRequestLogRepository { get; }
        IVerificationRequestLogRepository VerificationRequestLogRepository { get; }

        public LicensingService(AuthContext context,
            IEncryptionService encryptionService,
            IAccountRepository accountRepository,
            IMachineRepository machineRepository,
            IUniqueMinerRepository uniqueMinerRepository,
            IActivationCodeRepository activationCodeRepository,
            IMachineActivationRepository machineActivationRepository,
            IActivationRequestLogRepository activationRequestLogRepository,
            IVerificationRequestLogRepository verificationRequestLogRepository)
            : base(context)
        {
            EncryptionService = encryptionService;
            AccountRepository = accountRepository;
            MachineRepository = machineRepository;
            UniqueMinerRepository = uniqueMinerRepository;
            ActivationCodeRepository = activationCodeRepository;
            MachineActivationRepository = machineActivationRepository;
            ActivationRequestLogRepository = activationRequestLogRepository;
            VerificationRequestLogRepository = verificationRequestLogRepository;
        }

        public ServiceResponse ProcessActivation(ActivationRequest request)
        {
            Logger.Log("LicensingService.ProcessActivation - START");

            var logEntryResponse = LogActivationRequest(request);
            if (!logEntryResponse.IsSuccess)
            {
                Logger.Log($"logEntryResponse.ErrorMessage == {logEntryResponse.ErrorDetails.Message}");
                return logEntryResponse;
            }

            var getActivCodeResponse = GetValidMatchingActivationCode(request);
            if (!getActivCodeResponse.IsSuccess)
            {
                UpdateActivationRequestLogWithErrorDetails(logEntryResponse.ResponseDTO, getActivCodeResponse.ErrorDetails);
                return getActivCodeResponse;
            }

            var redeemRegCodeResponse = RedeemActivationCode(getActivCodeResponse.ResponseDTO, request);
            if (!redeemRegCodeResponse.IsSuccess)
                UpdateActivationRequestLogWithErrorDetails(logEntryResponse.ResponseDTO, redeemRegCodeResponse.ErrorDetails);
            else UpdateActivationRequestLogWithSuccess(logEntryResponse.ResponseDTO);

            Logger.Log("LicensingService.ProcessActivation - END");
            return redeemRegCodeResponse;
        }

        ServiceResponse<ActivationRequestLog> LogActivationRequest(ActivationRequest request) => TryExecute(() =>
        {
            var logEntry = new ActivationRequestLog
            {
                ActivationCode = request.ActivationCode,
                AccountName = request.AccountName,
                PcIdentifier = request.PcIdentifier,
                ReceivedDate = DateTime.UtcNow
            };
            logEntry = ActivationRequestLogRepository.Insert(logEntry);

            return ServiceResponse.Success(logEntry);
        });

        ServiceResponse UpdateActivationRequestLogWithErrorDetails(ActivationRequestLog logEntry, ErrorDetails errorDetails) => TryExecute(() =>
        {
            logEntry.ErrorCode = errorDetails.Code;
            logEntry.ErrorMessage = errorDetails.Message;
            ActivationRequestLogRepository.Update(logEntry);
            return ServiceResponse.Success();
        });
        ServiceResponse UpdateActivationRequestLogWithSuccess(ActivationRequestLog logEntry) => TryExecute(() =>
        {
            logEntry.Successful = true;
            ActivationRequestLogRepository.Update(logEntry);
            return ServiceResponse.Success();
        });

        ServiceResponse<ActivationCode> GetValidMatchingActivationCode(ActivationRequest request) => TryExecute(() =>
        {
            var activationCode = ActivationCodeRepository.GetByValue(request.ActivationCode);

            if (activationCode == null)
                return ServiceResponse<ActivationCode>.Error(StatusCodes.Status403Forbidden, "Provided activation code does not exist.");

            if (!activationCode.Available)
                return ServiceResponse<ActivationCode>.Error(StatusCodes.Status403Forbidden, "Provided activation code is not available anymore.");

            if (activationCode.ExpirationDate != null && activationCode.ExpirationDate < DateTime.UtcNow)
                return ServiceResponse<ActivationCode>.Error(StatusCodes.Status403Forbidden, "Provided activation code has expired.");

            return ServiceResponse<ActivationCode>.Success(activationCode);
        });

        ServiceResponse RedeemActivationCode(ActivationCode activationCode, ActivationRequest request)
            => TryExecute(inTransaction: true, action: () =>
        {
            var account = AccountRepository.GetByAccountName(request.AccountName);
            var isNewAccount = account == null;

            if (isNewAccount && !activationCode.ActivationTypeId.IsOneOf(ActivationType.Registration))
                return ServiceResponse.Error(StatusCodes.Status403Forbidden, "Provided activation code won't allow for registering new account.");
            else if (!isNewAccount && !activationCode.ActivationTypeId.IsOneOf(ActivationType.Continuation))
                return ServiceResponse.Error(StatusCodes.Status403Forbidden, "Provided activation code won't allow for continuing existing account.");

            activationCode.Available = false;
            ActivationCodeRepository.Update(activationCode);

            var activationDate = DateTime.UtcNow;
            var expirationDate = activationDate + TimeSpan.FromDays(activationCode.DaysGranted);

            if (isNewAccount)
            {
                account = new Account
                {
                    AccountName = request.AccountName,
                    RegistrationDate = activationDate,
                    Active = true
                };
                account = AccountRepository.Insert(account);
            }
            else if (!account.Active)
                return ServiceResponse.Error(StatusCodes.Status403Forbidden, "This account is inactive.");

            var machine = MachineRepository.GetByPcIdentifier(request.PcIdentifier);
            var isNewMachine = machine == null;

            if (!isNewMachine && machine.AccountId != account.Id)
                return ServiceResponse.Error(StatusCodes.Status403Forbidden, "This machine is already bound to another account.");


            if (isNewMachine)
            {
                machine = new Machine
                {
                    PcIdentifier = request.PcIdentifier,
                    AccountId = account.Id,
                    Active = true,
                    ExpirationDate = expirationDate
                };
                machine = MachineRepository.Insert(machine);
            }
            else
            {
                if (!machine.Active)
                    return ServiceResponse.Error(StatusCodes.Status403Forbidden, "This machine is inactive.");

                machine.ExpirationDate = expirationDate;
                MachineRepository.Update(machine);
            }

            var machineActivation = new MachineActivation
            {
                MachineId = machine.Id,
                ActivationCodeId = activationCode.Id,
                ActivationDate = activationDate
            };
            MachineActivationRepository.Insert(machineActivation);

            return ServiceResponse.Success();
        });

        public ServiceResponse ProcessVerification(VerifyLicenseRequest request)
        {
            Logger.Log("LicensingService.ProcessVerification - START");

            var logEntryResponse = LogVerificationRequest(request);
            if (!logEntryResponse.IsSuccess)
            {
                Logger.Log($"logEntryResponse.ErrorMessage == {logEntryResponse.ErrorDetails.Message}");
                return logEntryResponse;
            }
            var logEntry = logEntryResponse.ResponseDTO;

            bool verificationFailed = false;
            var reasons = new List<string>();

            var response = TryExecute(() =>
            {
                var account = AccountRepository.GetByAccountName(request.AccountName);
                var machine = MachineRepository.GetByPcIdentifier(request.PcIdentifier);

                var minerSignature = account == null || machine == null || request.MinerSignature == null ?
                    null : DecryptMinerSignature(account.Id, machine.Id, request.MinerSignature);
                if (minerSignature == null)
                {
                    verificationFailed = true;
                    reasons.Add("Undecryptable unique miner signature");
                }
                else
                {
                    logEntry.MinerSignature = minerSignature;
                    VerificationRequestLogRepository.Update(logEntry);
                }

                var uniqueMiner = minerSignature == null ? null : UniqueMinerRepository.GetBySignature(minerSignature);

                // ================== CONDITIONS ==================
                // account, machine & unique miner all must exist
                // account and machine must both be active
                // machine must be owned by this account
                // unique miner must be bound to this machine
                // machine must be unexpired
                // secret must be correct
                // ================================================

                if (account == null)
                {
                    verificationFailed = true;
                    reasons.Add("Account with given name does not exist");
                }
                else if (!account.Active)
                {
                    verificationFailed = true;
                    reasons.Add("Account with given name is not active");
                }

                if (machine == null)
                {
                    verificationFailed = true;
                    reasons.Add("Machine with given PC-Id does not exist");
                }
                else
                {
                    if (!machine.Active)
                    {
                        verificationFailed = true;
                        reasons.Add("Machine with given PC-Id is not active");
                    }
                    if (DateTime.UtcNow > machine.ExpirationDate)
                    {
                        verificationFailed = true;
                        reasons.Add("Machine with given PC-Id has expired");
                    }
                    if (account != null && machine.AccountId != account.Id)
                    {
                        verificationFailed = true;
                        reasons.Add("Account with given name does not own the machine with given PC-Id");
                    }
                }

                if (minerSignature != null)
                {
                    if (uniqueMiner == null)
                    {
                        verificationFailed = true;
                        reasons.Add("Unique miner with given signature does not exist");
                    }
                    else if (machine != null && uniqueMiner.MachineId != machine.Id)
                    {
                        verificationFailed = true;
                        reasons.Add("Unique miner with given signature does not belong to the machine with given PC-Id");
                    }
                }

                if (account != null && machine != null && minerSignature != null &&
                    !VerifySecret(request, account.Id, machine.Id, minerSignature))
                {
                    verificationFailed = true;
                    reasons.Add("Provided secret is incorrect.");
                }

                return ServiceResponse.Success();
            });

            if (!response.IsSuccess)
            {
                verificationFailed = true;
                reasons.Add(response.ErrorDetails.Message);
                logEntry.ErrorCode = response.ErrorDetails.Code;
            }
            else if (verificationFailed)
                logEntry.ErrorCode = 401;

            if (verificationFailed)
            {
                logEntry.ErrorMessage = string.Join(";\n", reasons);
                VerificationRequestLogRepository.Update(logEntry);
                if (logEntry.ErrorCode == 401)
                    return ServiceResponse.Unauthorized();
                return ServiceResponse.InternalServerError();
            }

            UpdateVerificationRequestLogWithSuccess(logEntry);
            Logger.Log("LicensingService.ProcessVerification - END");
            return response;
        }

        ServiceResponse<VerificationRequestLog> LogVerificationRequest(VerifyLicenseRequest request) => TryExecute(() =>
        {
            var logEntry = new VerificationRequestLog
            {
                AccountName = request.AccountName,
                PcIdentifier = request.PcIdentifier,
                EncryptedMinerSignature = request.MinerSignature,
                SentSecret = request.Secret,
                ReceivedDate = DateTime.UtcNow
            };
            logEntry = VerificationRequestLogRepository.Insert(logEntry);

            return ServiceResponse.Success(logEntry);
        });
        ServiceResponse UpdateVerificationRequestLogWithSuccess(VerificationRequestLog logEntry) => TryExecute(() =>
        {
            logEntry.Successful = true;
            VerificationRequestLogRepository.Update(logEntry);
            return ServiceResponse.Success();
        });

        string DecryptMinerSignature(int accountId, int machineId, string minerSignature)
        {
            var signatureBytes = Convert.FromBase64String(minerSignature);
            var decryptResponse = EncryptionService.RSADecrypt(accountId, machineId, signatureBytes);
            if (!decryptResponse.IsSuccess)
                return null;

            var stampedSignature = Encoding.UTF8.GetString(decryptResponse.ResponseDTO);
            var signature = stampedSignature.Substring(0, 32);
            var dateStr = stampedSignature.Substring(32);
            var dateSent = DateTime.Parse(dateStr);
            if (dateSent + TimeSpan.FromMinutes(5) < DateTime.UtcNow)
                return null;

            return signature;
        }

        bool VerifySecret(VerifyLicenseRequest request, int accountId, int machineId, string minerSignature)
        {
            var init = $"{request.AccountName}{request.PcIdentifier}{minerSignature}";
            var initBytes = Encoding.UTF8.GetBytes(init);
            var signResposne = EncryptionService.RSASign(accountId, machineId, initBytes);
            if (!signResposne.IsSuccess)
                return false;

            var secretBytes = Convert.FromBase64String(request.Secret);
            var decryptResponse = EncryptionService.RSADecrypt(accountId, machineId, secretBytes);
            if (!decryptResponse.IsSuccess)
                return false;

            var salt = new byte[16];
            Array.Copy(decryptResponse.ResponseDTO, 0, salt, 0, 16);

            var base64Signature = Convert.ToBase64String(signResposne.ResponseDTO);
            using (var pbkdf2 = new Rfc2898DeriveBytes(base64Signature, salt, 100000))
            {
                var hash = pbkdf2.GetBytes(128);

                for (int i = 0; i < 128; i++)
                    if (decryptResponse.ResponseDTO[i + 16] != hash[i])
                        return false;
            }

            return true;
        }

        public ServiceResponse<UnlockTokenResponse> GenerateUnlockToken(VerifyLicenseRequest request) => TryExecute(() =>
        {
            var account = AccountRepository.GetByAccountName(request.AccountName);
            var machine = MachineRepository.GetByPcIdentifier(request.PcIdentifier);
            var minerSignature = DecryptMinerSignature(account.Id, machine.Id, request.MinerSignature);
            if (minerSignature == null)
                return ServiceResponse.InternalServerError<UnlockTokenResponse>();

            var tokenExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            var strTokenExpiration = tokenExpiration.ToString("yyyy-MM-ddTHH:mm:ss");
            var init = $"{request.AccountName}{request.PcIdentifier}{minerSignature}{strTokenExpiration}";
            var initBytes = Encoding.UTF8.GetBytes(init);
            var signature = EncryptionService.RSASign(account.Id, machine.Id, initBytes);
            if (!signature.IsSuccess)
                return ServiceResponse.InternalServerError<UnlockTokenResponse>();
            var base64Signature = Convert.ToBase64String(signature.ResponseDTO);

            byte[] salt;
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt = new byte[16]);

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(base64Signature, salt, 100000))
                hash = pbkdf2.GetBytes(128);

            byte[] hashBytes = new byte[144];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 128);

            string secret = Convert.ToBase64String(hashBytes);
            return ServiceResponse.Success(new UnlockTokenResponse
            {
                PcIdentifier = request.PcIdentifier,
                AccountName = request.AccountName,
                TokenExpiration = strTokenExpiration,
                Secret = secret
            });
        });
    }
}
