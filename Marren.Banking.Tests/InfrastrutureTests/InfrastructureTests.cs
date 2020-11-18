using Marren.Banking.Domain.Model;
using Marren.Banking.Infrastructure.Contexts;
using Marren.Banking.Infrastructure.Services;
using Marren.Banking.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Marren.Banking.Domain.Services;
using Marren.Banking.Domain.Kernel;
using System.Threading;

namespace Marren.Banking.Tests.InfrastrutureTests
{
    public class InfrastructureTests
    {
        private readonly BankingAccountContext context;
        private readonly BankingAccountRepository repository;
        private readonly AuthService authService;
        private readonly FinanceService finService;
        private readonly AccountService accountService;

        public InfrastructureTests()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlite("Data Source=Testes.db");

            this.context = new BankingAccountContext(builder.Options);
            this.context.Database.EnsureDeleted();
            this.context.Database.EnsureCreated();

            this.repository = new BankingAccountRepository(this.context);

            this.authService = new AuthService();
            this.finService = new FinanceService();

            this.accountService = new AccountService(this.repository, this.finService, this.authService);
        }

        /// <summary>
        /// Testa abertura de conta no dia atual
        /// </summary>
        /// <returns>Async</returns>
        [Test]
        public async Task OpenAccountTodayTest()
        {
            var account = await this.accountService.OpenAccount("Maico", 0, 0, "AAA", DateTime.Now, 0);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            var balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance == 0, "Não ficou com zero de balanço");

            account = await this.accountService.OpenAccount("Maico 2", 0, 0, "AAA", DateTime.Now, 10);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance == 10, "Não ficou com 10 de balanço");

            account = await this.accountService.OpenAccount("Maico 3", 0, 0, "AAA", DateTime.Now, -10);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance == -10, "Não ficou com -10 de balanço");

        }

        /// <summary>
        /// Testa abertura de conta retroativa a 7 dias
        /// 
        /// Usa o WS do BACEN para calcular juros.
        /// Para o teste de exatidão deve-se usar mock.
        /// Este teste testa a infra do ws do bacen.
        /// </summary>
        /// <returns>Async</returns>
        [Test]
        public async Task OpenAccountLastWeekTest()
        {
            DateTime lastWeek = DateTime.Now.AddDays(-7);

            var account = await this.accountService.OpenAccount("Maico", 0, 0, "AAA", lastWeek, 0);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            var balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance == 0, "Não ficou com zero de balanço");

            account = await this.accountService.OpenAccount("Maico 2", 0, 0, "AAA", lastWeek, 1000);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance > 1000, "Não calculou os juros");

            account = await this.accountService.OpenAccount("Maico 3", 0, 0.0011m, "AAA", lastWeek, -1000);
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            balance = await this.accountService.GetBalance(account.Id);
            Assert.IsTrue(balance < -1000, "Não calculou as taxas do cheque especial");

        }

        /// <summary>
        /// Testa o saque sem mock
        /// </summary>
        [Test]
        public void WithdrawTest()
        {
            DateTime lastWeek = DateTime.Now.AddDays(-7);

            Account account = null;
            Assert.DoesNotThrowAsync(async () => account = await this.accountService.OpenAccount("Maico", 10, 0.012m, "AAA", lastWeek, 1000), "Nao conseguiu abrir conta");
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            var balance = 0m;
            Assert.DoesNotThrowAsync(async () => balance = await this.accountService.GetBalance(account.Id), "Não conseguiu verificar o saldo pela primeira vez.");
            Assert.IsTrue(balance > 1000, "Não calculou certo os juros");

            Assert.DoesNotThrowAsync(async () => balance = await this.accountService.Withdraw(account.Id, 200, "AAA"), "Não conseguiu sacar 1");
            Assert.DoesNotThrowAsync(async () => balance = await this.accountService.Withdraw(account.Id, 10+balance, "AAA"), "Não conseguiu sacar 2");

            Assert.IsTrue(balance == -10, "Não zerou o saldo");

            Assert.CatchAsync<BankingDomainException>(() => this.accountService.Withdraw(account.Id, 1, "AAA"), "Teste sem fundos");

            IEnumerable<Transaction> transactions = null;
            Assert.DoesNotThrowAsync(async () => transactions = await this.accountService.GetStatement(account.Id, lastWeek, null), "Não gerou extrato");

            Assert.IsTrue(balance == transactions.Last().Balance, "Saldo diferente no estrato");
        }

        /// <summary>
        /// Testa o depósito
        /// </summary>
        [Test]
        public void DepositTest()
        {
            DateTime lastWeek = DateTime.Now.AddDays(-7);

            Account account = null;
            Assert.DoesNotThrowAsync(async () => account = await this.accountService.OpenAccount("Maico", 100, 0.012m, "AAA", lastWeek, 1000), "Nao conseguiu abrir conta");
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            var balance = 0m;
            Assert.DoesNotThrowAsync(async () => balance = await this.accountService.GetBalance(account.Id), "Não conseguiu verificar o saldo pela primeira vez.");
            Assert.IsTrue(balance > 1000, "Não calculou certo os juros");
            var newBalance = 0m;
            Assert.DoesNotThrowAsync(async () => newBalance = await this.accountService.Deposit(account.Id, 200), "Não conseguiu depositar.");
            Assert.IsTrue(balance + 200 == newBalance, "Depositou mais o saldo não ficou correto");

        }

        [Test]
        public void AuthorizeTest()
        {
            Account account = null;
            Assert.DoesNotThrowAsync(async () => account = await this.accountService.OpenAccount("Maico", 10, 0.012m, "AAA", DateTime.Now, 10), "Nao conseguiu abrir conta");
            Assert.IsNotNull(account, "Não abriu a conta");
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            Assert.CatchAsync<BankingDomainException>(async () => await this.accountService.Authorize(account.Id, "BBB"), "Deixou autorizar com senha errada");

        }

        /// <summary>
        /// Sqlite não suporta bem operações ASYNC.
        /// A concorrência no SQLLITE não funciona.
        /// Testar no mysql. Trabalho futuro
        /// 
        /// Fonte:https://docs.microsoft.com/pt-br/dotnet/standard/data/sqlite/async
        /// 
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task ConcurrencyTest()
        {
            Account account = null;
            Assert.DoesNotThrowAsync(async () => account = await this.accountService.OpenAccount("Maico", 10, 0.012m, "AAA", DateTime.Now, 2000), "Nao conseguiu abrir conta");
            Assert.IsNotNull(account, "Não abriu a conta");
            int accid = account.Id;
            Assert.IsTrue(!account.IsTransient(), "Não obteve o ID da conta");
            List<Thread> pool = new List<Thread>();
            Transaction first = await this.repository.GetLastTransaction(accid);
            System.Collections.Concurrent.ConcurrentBag<int> erros = new System.Collections.Concurrent.ConcurrentBag<int>();
            System.Collections.Concurrent.ConcurrentBag<int> sucessos = new System.Collections.Concurrent.ConcurrentBag<int>();
            
            for (int i = 0; i < 10; i++)
            {
                pool.Add(new Thread(async () =>
                {
                    var builder = new DbContextOptionsBuilder();
                    builder.UseSqlite("Data Source=Testes.db");

                    using var ctx = new BankingAccountContext(builder.Options);
                    var rep = new BankingAccountRepository(ctx);
                    var aus = new AuthService();
                    var fs = new FinanceService();

                    var assrv = new AccountService(rep, fs, aus);

                    int errorCount = 0;
                    int sucessoCount = 0;
                    decimal balance = await assrv.GetBalance(accid);
                    do
                    {
                        try
                        {
                            balance = await assrv.Withdraw(accid, 1, "AAA");
                            sucessoCount++;
                        }
                        catch
                        {
                            errorCount++;
                            if (errorCount > 10000)
                            {
                                break;
                            }
                        }
                    } 
                    while (balance > 0);
                    erros.Add(errorCount);
                    sucessos.Add(sucessoCount);
                }));
            }

            pool.ForEach(a => a.Start());
            pool.ForEach(a => a.Join());

            var builder2 = new DbContextOptionsBuilder();
            builder2.UseSqlite("Data Source=Testes.db");
            using var ctx2 = new BankingAccountContext(builder2.Options);
            var first2 = await ctx2.Transactions.FirstOrDefaultAsync(x => x.Id == first.Id);
            Assert.NotNull(first2.NextTransaction); //Sempre vai falhar.
        }
    }
}
