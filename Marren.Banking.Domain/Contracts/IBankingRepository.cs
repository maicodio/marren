using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Contracts 
{
    /// <summary>
    /// Contrato do reposit�rio usado pode este dom�nio
    /// </summary>
    public interface IBankingRepository
    {
        /// <summary>
        /// Adiciona (INSERT) uma nova conta no reposit�rio
        /// </summary>
        /// <param name="account">Dados da conta</param>
        /// <returns>Async</returns>
        Task AddAccount(Account account);

        /// <summary>
        /// Obt�m uma conta buscando pelo id e hash da senha
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <param name="passwordHash">hash da senha</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        Task<Account> GetAccountByIdAndHash(int accountId, string passwordHash);

        /// <summary>
        /// Obt�m uma conta por id.
        /// Usada para obter informa��es de taxas e limites 
        /// ao realizar saques.
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        Task<Account> GetAccountById(int accountId);

        /// <summary>
        /// Adiciona (INSERT) um movimento (Transaction)
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>Async</returns>
        Task AddTransaction(Transaction transaction);

        /// <summary>
        /// Obt�m a �ltima transa��o da conta
        /// � a transa��o que cont�m o saldo da conta atual 
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <returns>Asyncronamente, retorna null ou a transa��o encontrada</returns>
        Task<Transaction> GetLastTransaction(int accountId);

        /// <summary>
        /// Obt�m as transa��es de uma conta, por per�odo
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <param name="init">data inicio</param>
        /// <param name="end">data fim</param>
        /// <returns>Asyncronamente as transa��es encontradas</returns>
        Task<IEnumerable<Transaction>> GetTransactions(int accountId, DateTime init, DateTime? end);

        /// <summary>
        /// Salva as altera��es at� agora realizadas.
        /// Conceiro de UnitOfWork.
        /// </summary>
        /// <returns>Async</returns>
        Task SaveChanges();

    }
}