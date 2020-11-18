using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Marren.Banking.Application.ViewModel;
using Marren.Banking.Domain.Contracts;
using Marren.Banking.Domain.Kernel;
using Marren.Banking.Domain.Services;
using Marren.Banking.Infrastructure.Contexts;
using Marren.Banking.Infrastructure.Repositories;
using Marren.Banking.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Marren.Banking.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankingAccountController : ControllerBase
    {
        /// <summary>Account Service da Aplicação</summary>
        private readonly AccountService service;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="context">Contexto injetado</param>
        /// <param name="financeService">Serviço financeiro injetado</param>
        /// <param name="authService">Serviço de Autorização injetado</param>
        public BankingAccountController(
            BankingAccountContext context,
            IFinanceService financeService,
            IAuthService authService)
        {
            //A aplicação monta o serviço do domínio com a infra estrutura necessária:
            this.service = new AccountService(new BankingAccountRepository(context), financeService, authService);
        }


        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] Login login)
        {
            try
            {
                var account = await this.service.Authorize(login.AccountId, login.Password);
                var token = AuthService.GenerateToken(account);

                return Result.Create<dynamic>(
                    new {
                        Ok = true,
                        account.Name,
                        account.Id,
                        Token = token
                    });
            }
            catch(BankingDomainException ex)
            {
                return Result.Create<dynamic>(ex);
            }
        }

        [HttpPost]
        [Route("create")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> CreateAccount([FromBody] CreateAccount accountData)
        {
            try
            {
                var account = await this.service.OpenAccount(
                    accountData.Name, 
                    accountData.OverdraftLimit,
                    accountData.OverdraftTax,
                    accountData.Password,
                    accountData.OpeningDate,
                    accountData.initialDeposit
                    );

                var token = AuthService.GenerateToken(account);

                return Result.Create<dynamic>(
                   new
                   {
                       Ok = true,
                       account.Name,
                       account.Id,
                       Token = token
                   });
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<dynamic>(ex);
            }
        }


        [HttpPost]
        [Route("statement")]
        [Authorize]
        public async Task<Result<IEnumerable<Statement>>> GetStatment([FromBody] DateTime? start)
        {
            try
            {
                var accountId = User.Claims.Where(x => x.Type == "marren_account_id").Select(x => int.Parse(x.Value)).FirstOrDefault();
                var data = await this.service.GetStatement(accountId, start.GetValueOrDefault(DateTime.Now.AddMonths(-1)), null);

                return Result.Create<IEnumerable<Statement>>(data.Select(d => new Statement
                {
                    Date = d.Date,
                    Balance = d.Balance,
                    Type = d.Type.Name + (string.IsNullOrWhiteSpace(d.Reference) ? string.Empty : string.Concat(" ", d.Reference)),
                    Value = d.Value
                }).ToArray());
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<IEnumerable<Statement>>(ex);
            }
        }

        [HttpPost]
        [Route("balance")]
        [Authorize]
        public async Task<Result<decimal>> GetBalance()
        {
            try
            {
                var accountId = User.Claims.Where(x => x.Type == "marren_account_id").Select(x => int.Parse(x.Value)).FirstOrDefault();
                var data = await this.service.GetBalance(accountId);
                return Result.Create<decimal>(data);
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<decimal>(ex);
            }
        }

        [HttpPost]
        [Route("withdraw")]
        [Authorize]
        public async Task<Result<decimal>> Withdraw([FromBody] Withdraw data)
        {
            try
            {
                var accountId = User.Claims.Where(x => x.Type == "marren_account_id").Select(x => int.Parse(x.Value)).FirstOrDefault();
                var balance = await this.service.Withdraw(accountId, data.Ammount, data.Password);
                return Result.Create(balance);
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<decimal>(ex);
            }
        }

        [HttpPost]
        [Route("transfer")]
        [Authorize]
        public async Task<Result<decimal>> Transfer([FromBody] Transfer data)
        {
            try
            {
                var accountId = User.Claims.Where(x => x.Type == "marren_account_id").Select(x => int.Parse(x.Value)).FirstOrDefault();
                var balance = await this.service.Transfer(accountId, data.Ammount, data.Password, data.AccountIdDeposit);
                return Result.Create(balance);
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<decimal>(ex);
            }
        }

        [HttpPost]
        [Route("deposit")]
        [Authorize]
        public async Task<Result<decimal>> Deposit([FromBody] decimal amount)
        {
            try
            {
                var accountId = User.Claims.Where(x => x.Type == "marren_account_id").Select(x => int.Parse(x.Value)).FirstOrDefault();
                var balance = await this.service.Deposit(accountId, amount);
                return Result.Create(balance);
            }
            catch (BankingDomainException ex)
            {
                return Result.Create<decimal>(ex);
            }
        }
    }
}
