using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GW2Vault.Auth.Model;
using Microsoft.EntityFrameworkCore;

namespace GW2Vault.Auth.Data
{
    public class AuthContext : DbContext
    {
        #region Static part

        static Dictionary<string, Func<AuthContext, object>> dbSets =
            new Dictionary<string, Func<AuthContext, object>>();

        static AuthContext()
            => InitDbSetsAccessors();

        static void InitDbSetsAccessors()
        {
            var t = typeof(AuthContext);
            var dbSetTypes = typeof(DbSet<>);
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var dbSetProps = props.Where(p => p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericTypeDefinition() == dbSetTypes);
            foreach (var prop in dbSetProps)
                dbSets.Add(prop.Name, context => prop.GetValue(context));
        }

        #endregion

        public AuthContext(DbContextOptions<AuthContext> options)
            : base(options)
        {
        }

        public DbSet<T> GetTable<T>()
            where T : class
        {
            if (!dbSets.TryGetValue(typeof(T).Name, out var dbSet))
                throw new ArgumentException($"DbSet is not defined for the type {typeof(T).Name}.");
            return (DbSet<T>)dbSet.Invoke(this);
        }

        public DbSet<Account> Account { get; set; }
        public DbSet<Machine> Machine { get; set; }
        public DbSet<UniqueMiner> UniqueMiner { get; set; }
        public DbSet<ActivationCode> ActivationCode { get; set; }
        public DbSet<MachineActivation> MachineActivation { get; set; }

        public DbSet<Credentials> Credentials { get; set; }
        public DbSet<KeyPair> KeyPair { get; set; }
        public DbSet<ControllerActionKeyPair> ControllerActionKeyPair { get; set; }
        public DbSet<AccountMachineKeyPair> AccountMachineKeyPair { get; set; }

        public DbSet<ActivationRequestLog> ActivationRequestLog { get; set; }
        public DbSet<VerificationRequestLog> VerificationRequestLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (Startup.ProdBuild)
            {
                MapEntitiesToTables(modelBuilder);
                ConfigurePrimaryKeys(modelBuilder);
                ConfigureIndices(modelBuilder);
                ConfigureColumns(modelBuilder);
                ConfigureRelationships(modelBuilder);
            }
#pragma warning restore CS0162 // Unreachable code detected
        }

        private void MapEntitiesToTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<Machine>().ToTable("Machine");
            modelBuilder.Entity<UniqueMiner>().ToTable("UniqueMiner");
            modelBuilder.Entity<ActivationCode>().ToTable("ActivationCode");
            modelBuilder.Entity<MachineActivation>().ToTable("MachineActivation");

            modelBuilder.Entity<Credentials>().ToTable("Credentials");
            modelBuilder.Entity<KeyPair>().ToTable("KeyPair");
            modelBuilder.Entity<ControllerActionKeyPair>().ToTable("ControllerActionKeyPair");
            modelBuilder.Entity<AccountMachineKeyPair>().ToTable("AccountMachineKeyPair");

            modelBuilder.Entity<ActivationRequestLog>().ToTable("ActivationRequestLog");
            modelBuilder.Entity<VerificationRequestLog>().ToTable("VerificationRequestLog");
        }

        private void ConfigurePrimaryKeys(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<Machine>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<UniqueMiner>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<ActivationCode>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<MachineActivation>().HasKey(x => x.Id).HasName("PRIMARY");

            modelBuilder.Entity<Credentials>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<KeyPair>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<ControllerActionKeyPair>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<AccountMachineKeyPair>().HasKey(x => x.Id).HasName("PRIMARY");

            modelBuilder.Entity<ActivationRequestLog>().HasKey(x => x.Id).HasName("PRIMARY");
            modelBuilder.Entity<VerificationRequestLog>().HasKey(x => x.Id).HasName("PRIMARY");
        }

        private void ConfigureIndices(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().HasIndex(x => x.AccountName).IsUnique().HasDatabaseName("UNIQUE");
            modelBuilder.Entity<Machine>().HasIndex(x => x.PcIdentifier).IsUnique().HasDatabaseName("UNIQUE");
            modelBuilder.Entity<UniqueMiner>().HasIndex(x => x.Signature).IsUnique().HasDatabaseName("Signature_UNIQUE");
            modelBuilder.Entity<UniqueMiner>().HasIndex(x => x.DownloadFilename).IsUnique().HasDatabaseName("DownloadFilename_UNIQUE");
            modelBuilder.Entity<ActivationCode>().HasIndex(x => x.Value).IsUnique().HasDatabaseName("UNIQUE");

            modelBuilder.Entity<Credentials>().HasIndex(x => x.Username).IsUnique().HasDatabaseName("UNIQUE");
            modelBuilder.Entity<ControllerActionKeyPair>().HasIndex(
                x => new { x.ControllerName, x.ActionName, x.KeyPairTypeId }).IsUnique().HasDatabaseName("UNIQUE");
            modelBuilder.Entity<AccountMachineKeyPair>().HasIndex(
                x => new { x.AccountId, x.MachineId, x.KeyPairTypeId }).IsUnique().HasDatabaseName("UNIQUE");
        }

        private void ConfigureColumns(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<Account>().Property(x => x.AccountName).HasColumnType("varchar(20)").IsRequired();
            modelBuilder.Entity<Account>().Property(x => x.RegistrationDate).HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<Account>().Property(x => x.Active).HasColumnType("tinyint").IsRequired();
            
            modelBuilder.Entity<Machine>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<Machine>().Property(x => x.PcIdentifier).HasColumnType("varchar(60)").IsRequired();
            modelBuilder.Entity<Machine>().Property(x => x.AccountId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<Machine>().Property(x => x.Active).HasColumnType("tinyint").IsRequired();
            modelBuilder.Entity<Machine>().Property(x => x.ExpirationDate).HasColumnType("datetime");
            
            modelBuilder.Entity<UniqueMiner>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<UniqueMiner>().Property(x => x.Signature).HasColumnType("char(32)").IsRequired();
            modelBuilder.Entity<UniqueMiner>().Property(x => x.MachineId).HasColumnType("int").IsRequired();
            
            modelBuilder.Entity<ActivationCode>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<ActivationCode>().Property(x => x.Value).HasColumnType("char(16)").IsRequired();
            modelBuilder.Entity<ActivationCode>().Property(x => x.Available).HasColumnType("tinyint").IsRequired();
            modelBuilder.Entity<ActivationCode>().Property(x => x.ExpirationDate).HasColumnType("datetime");
            modelBuilder.Entity<ActivationCode>().Property(x => x.DaysGranted).HasColumnType("int").IsRequired();
            modelBuilder.Entity<ActivationCode>().Property(x => x.ActivationTypeId).HasColumnType("int").IsRequired();
            
            modelBuilder.Entity<MachineActivation>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<MachineActivation>().Property(x => x.ActivationCodeId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<MachineActivation>().Property(x => x.MachineId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<MachineActivation>().Property(x => x.ActivationDate).HasColumnType("datetime").IsRequired();
            
            modelBuilder.Entity<Credentials>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<Credentials>().Property(x => x.Username).HasColumnType("varchar(50)").IsRequired();
            modelBuilder.Entity<Credentials>().Property(x => x.Password).HasColumnType("varchar(20)").IsRequired();

            modelBuilder.Entity<KeyPair>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<KeyPair>().Property(x => x.PrivateKey).HasColumnType("varchar(2000)").IsRequired();
            modelBuilder.Entity<KeyPair>().Property(x => x.PublicKey).HasColumnType("varchar(1000)").IsRequired();
            
            modelBuilder.Entity<ControllerActionKeyPair>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<ControllerActionKeyPair>().Property(x => x.ControllerName).HasColumnType("varchar(20)").IsRequired();
            modelBuilder.Entity<ControllerActionKeyPair>().Property(x => x.ActionName).HasColumnType("varchar(20)").IsRequired();
            modelBuilder.Entity<ControllerActionKeyPair>().Property(x => x.KeyPairTypeId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<ControllerActionKeyPair>().Property(x => x.KeyPairId).HasColumnType("int").IsRequired();
            
            modelBuilder.Entity<AccountMachineKeyPair>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<AccountMachineKeyPair>().Property(x => x.AccountId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<AccountMachineKeyPair>().Property(x => x.MachineId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<AccountMachineKeyPair>().Property(x => x.KeyPairTypeId).HasColumnType("int").IsRequired();
            modelBuilder.Entity<AccountMachineKeyPair>().Property(x => x.KeyPairId).HasColumnType("int").IsRequired();
            
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.ActivationCode).HasColumnType("varchar(100)");
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.AccountName).HasColumnType("varchar(100)");
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.PcIdentifier).HasColumnType("varchar(100)");
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.ReceivedDate).HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.Successful).HasColumnType("tinyint").IsRequired();
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.ErrorCode).HasColumnType("int");
            modelBuilder.Entity<ActivationRequestLog>().Property(x => x.ErrorMessage).HasColumnType("blob");
            
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.Id).HasColumnType("int").UseMySqlIdentityColumn().IsRequired();
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.AccountName).HasColumnType("varchar(100)");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.PcIdentifier).HasColumnType("varchar(100)");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.EncryptedMinerSignature).HasColumnType("varchar(400)");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.MinerSignature).HasColumnType("char(32)");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.SentSecret).HasColumnType("varchar(400)");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.ReceivedDate).HasColumnType("datetime").IsRequired();
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.Successful).HasColumnType("tinyint").IsRequired();
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.ErrorCode).HasColumnType("int");
            modelBuilder.Entity<VerificationRequestLog>().Property(x => x.ErrorMessage).HasColumnType("blob");
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Machine>().HasOne<Account>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Machine_AccountId");
            modelBuilder.Entity<UniqueMiner>().HasOne<Machine>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_UniqueMiner_MachineId");
            modelBuilder.Entity<MachineActivation>().HasOne<ActivationCode>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.ActivationCodeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MachineActivation_ActivationCodeId");
            modelBuilder.Entity<MachineActivation>().HasOne<Machine>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MachineActivation_MachineId");
            
            modelBuilder.Entity<ControllerActionKeyPair>().HasOne<KeyPair>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.KeyPairId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ControllerActionKeyPair_KeyPairId");
            modelBuilder.Entity<AccountMachineKeyPair>().HasOne<KeyPair>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.KeyPairId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountMachineKeyPair_KeyPairId");
            modelBuilder.Entity<AccountMachineKeyPair>().HasOne<Account>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountMachineKeyPair_AccountId");
            modelBuilder.Entity<AccountMachineKeyPair>().HasOne<Machine>().WithMany()
                .HasPrincipalKey(x => x.Id).HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountMachineKeyPair_MachineId");
        }
    }
}
