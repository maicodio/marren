using Marren.Banking.Domain.Contracts;
using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Services
{
    /// <summary>
    /// Servi�o do dom�nio para os servi�os da conta corrente.
    /// </summary>
    public class AccountService
    {
        /// <summary>Reposit�rio</summary>
        private readonly IBankingRepository repository;
        /// <summary>Servi�o de autentica��o</summary>
        private readonly IAuthService authService;
        /// <summary>Servi�o financeiro</summary>
        private readonly IFinanceService financeService;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="repository">Reposit�rio</param>
        /// <param name="financeService">Servi�o Financeiro</param>
        /// <param name="authService">Servi�o de Autentica��o</param>
        public AccountService(IBankingRepository repository, IFinanceService financeService, IAuthService authService){
            this.repository = repository;
            this.authService = authService;
            this.financeService = financeService;
        }

        /// <summary>
        /// Abertura de conta
        /// </summary>
        /// <param name="name">Nome do t�tular da conta</param>
        /// <param name="overdraftLimit">Limite de cheque especial</param>
        /// <param name="overdraftTax">Taxa do cheque especial</param>
        /// <param name="password">Senha</param>
        /// <param name="date">Data de abertura</param>
        /// <param name="initialDeposit">Dep�sito inicial</param>
        /// <returns>Retorna a conta aberta assincronamente</returns>
        public async Task<Account> OpenAccount(string name, decimal overdraftLimit, decimal overdraftTax, string password, DateTime date, decimal initialDeposit = 0)
        {
            
            string passwordHash = await this.authService.GenerateHash(password);
            var account = new Account(name, overdraftLimit, overdraftTax, password, passwordHash, date);
            var initTransaction = new Transaction(account, date, TransactionType.Openning, initialDeposit, initialDeposit);

            await this.repository.AddAccount(account);
            await this.repository.AddTransaction(initTransaction);
            await this.repository.SaveChanges();
            return account;
        }

        /// <summary>
        /// Autentica uma conta com base no n�mero e na senha
        /// </summary>
        /// <param name="accountId">N�mero da conta</param>
        /// <param name="password">Senha</param>
        /// <returns>Retorna a conta autenticada assincronamente</returns>
        public async Task<Account> Authorize(int accountId, string password)
        {
            Account.ValidatePassword(password);
            string passwordHash = await this.authService.GenerateHash(password);
            var loggedAccount = await this.repository.GetAccountByIdAndHash(accountId, passwordHash) 
                ?? throw new BankingDomainException("Conta ou Senha Inv�lidos.");
            return loggedAccount;
        }

        /// <summary>
        /// Obt�m o saldo de uma conta
        /// </summary>
        /// <param name="accountId">N�mero da conta</param>
        /// <returns>O valor do saldo assincronamente</returns>
        public async Task<decimal> GetBalance(int accountId)
        {
            Account account = await this.repository.GetAccountById(accountId)
                 ?? throw new BankingDomainException("Conta inv�lida.");

            Transaction lastTransaction = await this.GetLastTransaction(account);

            if (lastTransaction.IsTransient())
            {
                await this.repository.SaveChanges();
            }

            return lastTransaction.Balance;
        }

        /// <summary>
        /// Obt�m o extrato da conta
        /// </summary>
        /// <param name="accountId">N�mero da conta</param>
        /// <param name="start">Filtro de data inicial</param>
        /// <param name="end">Filtro de data final</param>
        /// <returns>Lista de transa��es assincronamente</returns>
        public async Task<IEnumerable<Transaction>> GetStatement(int accountId, DateTime start, DateTime? end)
        {
            Transaction.ValidateStatementFilter(ref start, ref end);

            Account account = await this.repository.GetAccountById(accountId)
                ?? throw new BankingDomainException("Conta inv�lida.");

            var last = await this.GetLastTransaction(account);

            if (last.IsTransient())
            {
                await this.repository.SaveChanges();
            }

            return await this.repository.GetTransactions(accountId, start, end);
        }

        /// <summary>
        /// Saque
        /// </summary>
        /// <param name="accountId">N�mero da conta</param>
        /// <param name="value">Valor</param>
        /// <param name="password">Senha de confirma��o</param>
        /// <returns>Saldo atualizado assincronamente</returns>
        public async Task<decimal> Withdraw(int accountId, decimal value, string password)
        {
            Account.ValidatePassword(password);
            Account account = await this.Authorize(accountId, password);

            Transaction lastTransaction = await this.GetLastTransaction(account);
            Transaction newTransaction = lastTransaction.Withdraw(value);

            await this.repository.AddTransaction(newTransaction);
            await this.repository.SaveChanges();

            return newTransaction.Balance;
        }

        /// <summary>
        /// Dep�sito
        /// </summary>
        /// <param name="accountId">N�mero da conta</param>
        /// <param name="value">Valor</param>
        /// <returns>Saldo atualizado assincronamente</returns>
        public async Task<decimal> Deposit(int accountId, decimal value)
        {
            Account account = await this.repository.GetAccountById(accountId)
                 ?? throw new BankingDomainException("Conta inv�lida.");

            Transaction lastTransaction = await this.GetLastTransaction(account);
            Transaction newTransaction = lastTransaction.Deposit(value);

            await this.repository.AddTransaction(newTransaction);
            await this.repository.SaveChanges();

            return newTransaction.Balance;
        }

        /// <summary>
        /// Obt�m a �ltima transa��o (movimento) do dia de hoje.
        /// 
        /// Se a �ltima transa��o do cliente n�o for do dia de hoje,
        /// gera transa��es de saldo e de rendimentos/taxas at� chegar no dia de hoje.
        /// 
        /// Consulta o servi�o financeiro, para obter quais dias �teis e quais rendimentos
        /// que s�o devidos/cobrados da conta do cliente.
        /// </summary>
        /// <param name="account">COnta</param>
        /// <returns>�ltima transa��o do dia</returns>
        private async Task<Transaction> GetLastTransaction(Account account)
        {
            Transaction lastTransaction = await this.repository.GetLastTransaction(account.Id)
                ?? throw new BankingDomainException("Nenhuma transa��o encontrada.");

            Dictionary<string, decimal> interestRates = null;

            //Enquanto a �ltima transa��o da conta n�o for do dia atual
            while (lastTransaction.Date.Date < DateTime.Today)
            {
                //Se n�o obteve as taxas/dias do per�odo ainda
                if (interestRates == null)
                {
                    //Obt�m as taxas/dias do per�odo.
                    interestRates = await this.financeService.GetInterestRate(lastTransaction.Date, DateTime.Today);
                }

                //Obt�m os juros do dia. Se n�o houver considera feriado.
                interestRates.TryGetValue(lastTransaction.Date.ToString("yyyyMMdd"), out decimal interestRate);

                //Para cada transa��o gerada (saldo e rendimentos/taxas)
                foreach (var item in lastTransaction.GenerateNextDayBalance(interestRate, account.OverdraftTax))
                {
                    //Adiciona a transa��o no reposit�rio.
                    await this.repository.AddTransaction(item);
                    lastTransaction = item;
                }
                //� necess�rio salvar o saldo do dia a cada itera��o
                //para evitar uma lista encadeada muito longa.
                //Em caso de falha, a rotina tem consi��es de continuar daonde parou.
                await this.repository.SaveChanges();
            }
            return lastTransaction;
        }


    }
}