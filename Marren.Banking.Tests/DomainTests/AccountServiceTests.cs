using Marren.Banking.Domain.Contracts;
using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using Marren.Banking.Domain.Services;
using Marren.Banking.Infrastructure.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marren.Banking.Tests.DomainTests
{
    /// <summary>
    /// Testador mockado de account services do domínimo
    /// </summary>
    public class AccountServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Testa autorização com mock
        /// </summary>
        [Test]
        public void AuthorizeTests()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new DummyFinanceService(), new AuthService());
            
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Authorize(0, null), "Validação Senha nula");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Authorize(0, ""), "Validação Senha em branco");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Authorize(0, "2"), "Validação Senha curta");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Authorize(0, "2222"), "Validação Conta Inexistente");
            
            dummyRepo.GetAccountByIdAndHashResult = new Account("Teste", 100, 0.1m, "aaa", "aaa", DateTime.Today);
            Assert.DoesNotThrowAsync(async () => await accountService.Authorize(0, "AAAA"), "Senha Sucesso");
        }

        /// <summary>
        /// Abertura de conta com mock
        /// Campos inválidos
        /// </summary>
        [Test]
        public void OpenAccountTests()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new DummyFinanceService(), new AuthService());

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount(null, -10, -1, null, DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", -10, -1, null, DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", 0, -1, null, DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", 0, 0, null, DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", 0, 0, "", DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", 0, 0, "a", DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.OpenAccount("Maico", 0, 0, "aaaa", DateTime.MinValue, -10), "Validação Campos Inválidos");
            Assert.DoesNotThrowAsync(async () => await accountService.OpenAccount("Maico", 0, 0, "aaaa", DateTime.Now.Date, -10), "Sucesso");
        }

        /// <summary>
        /// Teste do saldo com mock
        /// Testa as taxas de juros
        /// </summary>
        [Test]
        public void GetBalanceTest()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new DummyFinanceService(), new AuthService());

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetBalance(1), "Conta não encontrada");

            dummyRepo.GetAccountByIdResult = new Account("Teste", 0, 0.012m, "aaaa", "aaaa", DateTime.Today);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetBalance(1), "Sem transações");

            dummyRepo.AddedTractions.Clear();
            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdResult, DateTime.Now.Date.AddDays(-2), TransactionType.Openning, 10, 10, null);
            decimal balance = 0;
            Assert.DoesNotThrowAsync(async () => { balance = await accountService.GetBalance(1); }, "Transações geradas");
            Assert.IsTrue(balance == 10.05m, "Não calculou juros selic.");
            Assert.IsTrue(dummyRepo.AddedTractions.Count == 3, "Deveria ser 4 transações");

            dummyRepo.AddedTractions.Clear();
            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdResult, DateTime.Now.Date.AddDays(-2), TransactionType.Openning, -10, -10, null);
            Assert.DoesNotThrowAsync(async () => { balance = await accountService.GetBalance(1); }, "Transações geradas");
            Assert.IsTrue(balance == -10.12m, "Não calculou juros cheque especial.");
            Assert.IsTrue(dummyRepo.AddedTractions.Count == 3, "Deveria ser 3 transações");

            dummyRepo.AddedTractions.Clear();
            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdResult, DateTime.Now.Date.AddDays(-2), TransactionType.Openning, 0, 0, null);
            Assert.DoesNotThrowAsync(async () => { balance = await accountService.GetBalance(1); }, "Transações geradas");
            Assert.IsTrue(balance == 0, "Saldo deveria estar zerado");

            Assert.IsTrue(dummyRepo.AddedTractions.Count == 2, "Deveria ser 2 transações");
        }

        /// <summary>
        /// Teste do deposito com mock
        /// </summary>
        [Test]
        public void DepositTests()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new DummyFinanceService(), new AuthService());

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Deposit(0, -10), "Conta inválida");

            dummyRepo.GetAccountByIdResult = new Account("Teste", 5, 0.012m, "aaa", "aaa", DateTime.Today);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Deposit(0, -10), "Conta sem transações");

            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdResult, DateTime.Now, TransactionType.Balance, 0, -10, null);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Deposit(0, -10), "Validação Valor Negativo");
            Assert.DoesNotThrowAsync(async () => await accountService.Deposit(0, 10), "Sucesso");
        }

        /// <summary>
        /// Teste do saque com mock
        /// </summary>
        [Test]
        public void WithdrawTests()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new FinanceService(), new AuthService());

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, -10, null), "Validacao da senha");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, -10, "0"), "Validacao da senha");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, -10, "00000"), "Validacao da conta");

            dummyRepo.GetAccountByIdAndHashResult = new Account("Teste", 5, 0.012m, "aaa", "aaa", DateTime.Today);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, -10, "00000"), "Validacao da transação");

            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdAndHashResult, DateTime.Now, TransactionType.Balance, 0, 10, null);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, -10, "00000"), "Validacao do Valor");

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.Withdraw(0, 16, "00000"), "Validacao do Saldo da Conta");

            Assert.DoesNotThrowAsync(async () => await accountService.Withdraw(0, 11, "00000"), "Sucesso.");

        }

        /// <summary>
        /// Teste do extrato (melhorar)
        /// </summary>
        [Test]
        public void GetStatementsTests()
        {
            var dummyRepo = new DummyBankingRepository();
            var accountService = new AccountService(dummyRepo, new FinanceService(), new AuthService());

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetStatement(0, DateTime.MinValue, DateTime.MinValue), "Datas inválidas");
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetStatement(0, DateTime.Now, DateTime.Now.AddDays(-2)), "Datas inválidas");

            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetStatement(0, DateTime.Now, null), "Conta inválida");

            dummyRepo.GetAccountByIdResult = new Account("Teste", 5, 0.012m, "aaa", "aaa", DateTime.Today);
            Assert.CatchAsync<BankingDomainException>(async () => await accountService.GetStatement(0, DateTime.MinValue, DateTime.MinValue), "Sem Transações");

            dummyRepo.GetLastTransactionResult = new Transaction(dummyRepo.GetAccountByIdResult, DateTime.Now, TransactionType.Balance, 0, -10, null);
            dummyRepo.GetTransactionsResult = new List<Transaction>();
            Assert.DoesNotThrowAsync(async () => await accountService.GetStatement(0, DateTime.Now, null), "Sucesso.");
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Mock do serviço financeiro
        /// </summary>
        class DummyFinanceService : IFinanceService
        {
            public async Task<Dictionary<string, decimal>> GetInterestRate(DateTime start, DateTime end)
            {
                int totalDays = (int)(end - start).TotalDays;
                return Enumerable.Range(1, totalDays).Select(x => new Tuple<DateTime, decimal>(start.AddDays(x), 0.005m)).ToDictionary(k => k.Item1.ToString("yyyyMMdd"), e => e.Item2);
            }
        }

        /// <summary>
        /// Mock do repositório
        /// </summary>
        class DummyBankingRepository : IBankingRepository
        {

            public Account GetAccountByIdResult { get; set; }
            public Transaction GetLastTransactionResult { get; set; }
            public Account GetAccountByIdAndHashResult { get; set; }
            public List<Transaction> GetTransactionsResult { get; set; }

            public List<Transaction> AddedTractions { get; set; } = new List<Transaction>();
            public List<Transaction> UpdatedTractions { get; set; } = new List<Transaction>();
            public List<Account> AddedAccounts { get; set; } = new List<Account>();


            public async Task AddAccount(Account account)
            {
                this.AddedAccounts.Add(account);
            }

            public async Task AddTransaction(Transaction transaction)
            {
                this.AddedTractions.Add(transaction);
            }

            public async Task<Account> GetAccountById(int accountId)
            {
                return GetAccountByIdResult;
            }

            public async Task<Account> GetAccountByIdAndHash(int accountId, string passwordHash)
            {
                return this.GetAccountByIdAndHashResult;
            }

            public async Task<Transaction> GetLastTransaction(int accountId)
            {
                return this.GetLastTransactionResult;
            }

            public async Task<IEnumerable<Transaction>> GetTransactions(int accountId, DateTime init, DateTime? end)
            {
                return this.GetTransactionsResult;
            }

            public async Task SaveChanges()
            {
                
            }

            public async Task UpdateTransaction(Transaction transaction)
            {
                this.UpdatedTractions.Add(transaction);
            }
        }
#pragma warning restore CS1998

    }
}
