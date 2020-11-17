using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Contracts 
{
    /// <summary>
    /// Contrato do repositório usado pode este domínio
    /// </summary>
    public interface IBankingRepository
    {
        /// <summary>
        /// Adiciona (INSERT) uma nova conta no repositório
        /// </summary>
        /// <param name="account">Dados da conta</param>
        /// <returns>Async</returns>
        Task AddAccount(Account account);

        /// <summary>
        /// Obtém uma conta buscando pelo id e hash da senha
        /// </summary>
        /// <param name="accountId">número da conta</param>
        /// <param name="passwordHash">hash da senha</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        Task<Account> GetAccountByIdAndHash(int accountId, string passwordHash);

        /// <summary>
        /// Obtém uma conta por id.
        /// Usada para obter informações de taxas e limites 
        /// ao realizar saques.
        /// </summary>
        /// <param name="accountId">número da conta</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        Task<Account> GetAccountById(int accountId);

        /// <summary>
        /// Adiciona (INSERT) um movimento (Transaction)
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>Async</returns>
        Task AddTransaction(Transaction transaction);

        /// <summary>
        /// Obtém a última transação da conta
        /// É a transação que contém o saldo da conta atual 
        /// </summary>
        /// <param name="accountId">número da conta</param>
        /// <returns>Asyncronamente, retorna null ou a transação encontrada</returns>
        Task<Transaction> GetLastTransaction(int accountId);

        /// <summary>
        /// Obtém as transações de uma conta, por período
        /// </summary>
        /// <param name="accountId">número da conta</param>
        /// <param name="init">data inicio</param>
        /// <param name="end">data fim</param>
        /// <returns>Asyncronamente as transações encontradas</returns>
        Task<IEnumerable<Transaction>> GetTransactions(int accountId, DateTime init, DateTime? end);

        /// <summary>
        /// Salva as alterações até agora realizadas.
        /// Conceiro de UnitOfWork.
        /// </summary>
        /// <returns>Async</returns>
        Task SaveChanges();

    }
}