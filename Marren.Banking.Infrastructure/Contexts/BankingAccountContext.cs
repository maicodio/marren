using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marren.Banking.Infrastructure.Contexts
{
    /// <summary>
    /// Context das contas correntes e transações
    /// </summary>
    public class BankingAccountContext: DbContext
    {
        /// <summary>
        /// Tabela de contas correntes
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        /// <summary>
        /// Tabela de transações
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="options">Opções do db context</param>
        public BankingAccountContext(DbContextOptions options) : base(options)
        { }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Account>().HasKey(m => m.Id);
            builder.Entity<Account>().Property(m => m.Id).ValueGeneratedOnAdd();

            builder.Entity<Transaction>().HasKey(m => m.Id);
            builder.Entity<Transaction>().Property(m => m.Id).ValueGeneratedOnAdd();
            builder.Entity<Transaction>().Property(m => m.Type).HasConversion(x=>x.Id, x=>Enumeration.FromId<TransactionType>(x));
            builder.Entity<Transaction>().HasOne(m => m.Account);
            builder.Entity<Transaction>().HasOne(m => m.NextTransaction);

            base.OnModelCreating(builder);
        }
    }
}
