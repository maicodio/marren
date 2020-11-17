using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marren.Banking.Domain.Model;
using Marren.Banking.Infrastructure.Contexts;

namespace Marren.Banking.Infrastructure.Repositories
{
    /// <summary>
    /// Reposit�rio para Contas Marren usando EF
    /// </summary>
    public class BankingAccountRepository : IBankingRepository
    {
        /// <summary>
        /// Contexto EF
        /// </summary>
        private readonly BankingAccountContext context;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="context">Contexto EF</param>
        public BankingAccountRepository(BankingAccountContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adiciona (INSERT) uma nova conta no reposit�rio
        /// </summary>
        /// <param name="account">Dados da conta</param>
        /// <returns>Async</returns>
        public async Task AddAccount(Account account)
        {
            await this.context.Accounts.AddAsync(account);
        }

        /// <summary>
        /// Adiciona (INSERT) um movimento (Transaction)
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>Async</returns>
        public async Task AddTransaction(Transaction transaction)
        {
            await this.context.Transactions.AddAsync(transaction);
        }

        /// <summary>
        /// Obt�m uma conta por id.
        /// Usada para obter informa��es de taxas e limites 
        /// ao realizar saques.
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        public async Task<Account> GetAccountById(int accountId)
        {
            return await this.context.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
        }

        /// <summary>
        /// Obt�m uma conta buscando pelo id e hash da senha
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <param name="passwordHash">hash da senha</param>
        /// <returns>Asyncronamente, retorna null ou a conta encontrada</returns>
        public async Task<Account> GetAccountByIdAndHash(int accountId, string passwordHash)
        {
            return await this.context.Accounts.FirstOrDefaultAsync(x => x.Id == accountId && x.PasswordHash == passwordHash);
        }

        /// <summary>
        /// Obt�m a �ltima transa��o da conta
        /// � a transa��o que cont�m o saldo da conta atual 
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <returns>Asyncronamente, retorna null ou a transa��o encontrada</returns>
        public async Task<Transaction> GetLastTransaction(int accountId)
        {
            var query = this.context.Transactions.Where(x => x.Account.Id == accountId);
            return await query.OrderByDescending(x => x.Date).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obt�m as transa��es de uma conta, por per�odo
        /// </summary>
        /// <param name="accountId">n�mero da conta</param>
        /// <param name="init">data inicio</param>
        /// <param name="end">data fim</param>
        /// <returns>Asyncronamente as transa��es encontradas</returns>
        public async Task<IEnumerable<Transaction>> GetTransactions(int accountId, DateTime init, DateTime? end)
        {
            var query = this.context.Transactions.Where(x => x.Account.Id == accountId && x.Date.Date >= init.Date);

            if (end.HasValue)
            {
                query = query.Where(x => x.Date.Date <= end.Value.Date);
            }

            return await query.OrderBy(x => x.Date).ToListAsync();
        }

        /// <summary>
        /// Salva as altera��es at� agora realizadas.
        /// Conceiro de UnitOfWork.
        /// </summary>
        /// <returns>Async</returns>
        public async Task SaveChanges()
        {
            await this.context.SaveChangesAsync();
        }
    }
}