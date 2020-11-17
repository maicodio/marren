using Marren.Banking.Domain.Contracts;
using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Services
{
    /// <summary>
    /// Serviço do domínio para os serviços da conta corrente.
    /// </summary>
    public class AccountService
    {
        /// <summary>Repositório</summary>
        private readonly IBankingRepository repository;
        /// <summary>Serviço de autenticação</summary>
        private readonly IAuthService authService;
        /// <summary>Serviço financeiro</summary>
        private readonly IFinanceService financeService;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="repository">Repositório</param>
        /// <param name="financeService">Serviço Financeiro</param>
        /// <param name="authService">Serviço de Autenticação</param>
        public AccountService(IBankingRepository repository, IFinanceService financeService, IAuthService authService){
            this.repository = repository;
            this.authService = authService;
            this.financeService = financeService;
        }

        /// <summary>
        /// Abertura de conta
        /// </summary>
        /// <param name="name">Nome do títular da conta</param>
        /// <param name="overdraftLimit">Limite de cheque especial</param>
        /// <param name="overdraftTax">Taxa do cheque especial</param>
        /// <param name="password">Senha</param>
        /// <param name="date">Data de abertura</param>
        /// <param name="initialDeposit">Depósito inicial</param>
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
        /// Autentica uma conta com base no número e na senha
        /// </summary>
        /// <param name="accountId">Número da conta</param>
        /// <param name="password">Senha</param>
        /// <returns>Retorna a conta autenticada assincronamente</returns>
        public async Task<Account> Authorize(int accountId, string password)
        {
            Account.ValidatePassword(password);
            string passwordHash = await this.authService.GenerateHash(password);
            var loggedAccount = await this.repository.GetAccountByIdAndHash(accountId, passwordHash) 
                ?? throw new BankingDomainException("Conta ou Senha Inválidos.");
            return loggedAccount;
        }

        /// <summary>
        /// Obtém o saldo de uma conta
        /// </summary>
        /// <param name="accountId">Número da conta</param>
        /// <returns>O valor do saldo assincronamente</returns>
        public async Task<decimal> GetBalance(int accountId)
        {
            Account account = await this.repository.GetAccountById(accountId)
                 ?? throw new BankingDomainException("Conta inválida.");

            Transaction lastTransaction = await this.GetLastTransaction(account);

            if (lastTransaction.IsTransient())
            {
                await this.repository.SaveChanges();
            }

            return lastTransaction.Balance;
        }

        /// <summary>
        /// Obtém o extrato da conta
        /// </summary>
        /// <param name="accountId">Número da conta</param>
        /// <param name="start">Filtro de data inicial</param>
        /// <param name="end">Filtro de data final</param>
        /// <returns>Lista de transações assincronamente</returns>
        public async Task<IEnumerable<Transaction>> GetStatement(int accountId, DateTime start, DateTime? end)
        {
            Transaction.ValidateStatementFilter(ref start, ref end);

            Account account = await this.repository.GetAccountById(accountId)
                ?? throw new BankingDomainException("Conta inválida.");

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
        /// <param name="accountId">Número da conta</param>
        /// <param name="value">Valor</param>
        /// <param name="password">Senha de confirmação</param>
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
        /// Depósito
        /// </summary>
        /// <param name="accountId">Número da conta</param>
        /// <param name="value">Valor</param>
        /// <returns>Saldo atualizado assincronamente</returns>
        public async Task<decimal> Deposit(int accountId, decimal value)
        {
            Account account = await this.repository.GetAccountById(accountId)
                 ?? throw new BankingDomainException("Conta inválida.");

            Transaction lastTransaction = await this.GetLastTransaction(account);
            Transaction newTransaction = lastTransaction.Deposit(value);

            await this.repository.AddTransaction(newTransaction);
            await this.repository.SaveChanges();

            return newTransaction.Balance;
        }

        /// <summary>
        /// Obtém a última transação (movimento) do dia de hoje.
        /// 
        /// Se a última transação do cliente não for do dia de hoje,
        /// gera transações de saldo e de rendimentos/taxas até chegar no dia de hoje.
        /// 
        /// Consulta o serviço financeiro, para obter quais dias úteis e quais rendimentos
        /// que são devidos/cobrados da conta do cliente.
        /// </summary>
        /// <param name="account">COnta</param>
        /// <returns>Última transação do dia</returns>
        private async Task<Transaction> GetLastTransaction(Account account)
        {
            Transaction lastTransaction = await this.repository.GetLastTransaction(account.Id)
                ?? throw new BankingDomainException("Nenhuma transação encontrada.");

            Dictionary<string, decimal> interestRates = null;

            //Enquanto a última transação da conta não for do dia atual
            while (lastTransaction.Date.Date < DateTime.Today)
            {
                //Se não obteve as taxas/dias do período ainda
                if (interestRates == null)
                {
                    //Obtém as taxas/dias do período.
                    interestRates = await this.financeService.GetInterestRate(lastTransaction.Date, DateTime.Today);
                }

                //Obtém os juros do dia. Se não houver considera feriado.
                interestRates.TryGetValue(lastTransaction.Date.ToString("yyyyMMdd"), out decimal interestRate);

                //Para cada transação gerada (saldo e rendimentos/taxas)
                foreach (var item in lastTransaction.GenerateNextDayBalance(interestRate, account.OverdraftTax))
                {
                    //Adiciona a transação no repositório.
                    await this.repository.AddTransaction(item);
                    lastTransaction = item;
                }
                //É necessário salvar o saldo do dia a cada iteração
                //para evitar uma lista encadeada muito longa.
                //Em caso de falha, a rotina tem consições de continuar daonde parou.
                await this.repository.SaveChanges();
            }
            return lastTransaction;
        }


    }
}