using Marren.Banking.Domain.Kernel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Marren.Banking.Domain.Model 
{
    /// <summary>
    /// Entidade que representa um movimento da conta corrente
    /// </summary>
    public class Transaction: Entity
    {
        /// <summary>
        /// Valor m�ximo de uma transa��o (movimento)
        /// </summary>
        private const decimal MAX_VALUE = 1000000000m;

        /// <summary>
        /// Conta corrente vinculada
        /// </summary>
        public Account Account { get; private set; }

        /// <summary>
        /// Data e hora da transa��o (movimento)
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Tipo de transa��o
        /// </summary>
        public TransactionType Type { get; private set; }

        /// <summary>
        /// Valor da transa��o.
        /// Pode ser negativo, de acordo com o tipo.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        /// Saldo da conta.
        /// </summary>
        public decimal Balance { get; private set; }

        /// <summary>
        /// Armazena o ID da proxima transa��o.
        /// </summary>
        public Transaction NextTransaction { get; private set; }

        /// <summary>
        /// Construtor interno
        /// </summary>
        protected Transaction()
        { }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="account">Conta corrente</param>
        /// <param name="date">Data da opera��o</param>
        /// <param name="type">Tipo de transa��o</param>
        /// <param name="value">Valor</param>
        /// <param name="balance">Saldo</param>
        /// <param name="nextTransaction">Pr�xima transa��o</param>
        public Transaction(Account account, DateTime date, TransactionType type, decimal value, decimal balance, Transaction nextTransaction=null): this()
        {
            var errors = new List<ValidationError>();

            this.Account = account;
            this.Date = date;
            this.Type = type;
            this.Value = value;
            this.Balance = balance;
            this.NextTransaction = nextTransaction;

            this.Validate(errors);

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Campos inv�lidos", errors.ToArray());
            }
        }

        /// <summary>
        /// Valida a transa��o
        /// </summary>
        /// <param name="errors">Lista de erros</param>
        private void Validate(IList<ValidationError> errors)
        {
            if (this.Account == null)
            {
                errors.Add(new ValidationError("Conta inv�lida", "Account", "Transaction"));
            }

            if (this.Date > DateTime.Now)
            {
                errors.Add(new ValidationError("Data da transfer�ncia inv�lida.", "Value", "Transaction"));
            }

            if (this.Type == null)
            {
                errors.Add(new ValidationError("Tipo de transfer�ncia n�o informado.", "Type", "Transaction"));
            }

            if (Enumeration.GetAll<TransactionType>().Any(x=>x.Id == this.Id))
            {
                errors.Add(new ValidationError("Tipo de transfer�ncia inv�lido", "Type", "Transaction"));
            }

            this.Value = Math.Round(this.Value, 2);
            this.Balance = Math.Round(this.Balance, 2);
        }

        /// <summary>
        /// M�todo para calcular o saldo do dia seguinte
        /// 
        /// O saldo da conta do cliente fica armazenado na �ltima transa��o realizada pelo cliente.
        /// 
        /// Quando se busca o saldo atualizado � necess�rio processar todos os dias desde a �ltima transa��o
        /// para calcular rendimentos e taxas da conta do cliente.
        /// 
        /// Sendo esta transa��o a �ltima transa��o da conta, e n�o sendo do dia de hoje, este m�todo calcula o saldo
        /// do dia seguinte pelos par�metros, gerando um movimento de saldo e um movimento de rendimento ou taxa.
        /// 
        /// O m�todo AccountService GetLastTransaction orquestra a obten��o desse saldo, chamando este m�todo enquanto
        /// n�o estiver com o saldo do dia.
        /// </summary>
        /// <param name="interestRate">Taxa de juros do dia</param>
        /// <param name="overdraftTax">Taxa do limite do cheque especial</param>
        /// <returns>Lista de transa��es do saldo do dia seguinte</returns>
        internal IEnumerable<Transaction> GenerateNextDayBalance(decimal interestRate, decimal overdraftTax)
        {
            DateTime newDate = this.Date.Date.AddDays(1);
            List<ValidationError> errors = new List<ValidationError>();

            if (this.NextTransaction != null)
            {
                errors.Add(new ValidationError("Est� n�o � a �ltima transa��o."));
            }

            if (interestRate < 0)
            {
                errors.Add(new ValidationError("Taxa de juros inv�lida."));
            }

            if (overdraftTax < 0)
            {
                errors.Add(new ValidationError("Taxa do cheque especial inv�lida."));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException($"Erro ao processar taxas da conta {this.Account.Id} para o dia {newDate:yyyy-MM-dd}", errors.ToArray());

            }

            Transaction tax = null;
            //interestRate == 0 significa que era feriado. N�o calcula juros nem taxas.
            if (interestRate > 0)
            {
                if (this.Balance > 0)
                {
                    decimal newBalance = this.Balance * interestRate;
                    tax = new Transaction(this.Account, newDate.AddMilliseconds(100), TransactionType.Interest, newBalance, this.Balance + newBalance);

                }
                else if (this.Balance < 0)
                {
                    decimal newBalance = this.Balance * overdraftTax;
                    tax = new Transaction(this.Account, newDate.AddMilliseconds(100), TransactionType.Fees, newBalance, this.Balance + newBalance);
                }
            }

            this.NextTransaction = new Transaction(this.Account, newDate, TransactionType.Balance, 0, this.Balance, tax);

            yield return this.NextTransaction;

            if (tax != null)
            {
                yield return tax;
            }

        }

        /// <summary>
        /// Saque
        /// 
        /// Sendo esta a �ltima transa��o do dia do cliente
        /// Realiza o saque, gerando uma nova transa��o e armazenando 
        /// o id da proxima transa��o nesta.
        /// </summary>
        /// <param name="value">Valor a ser sacado</param>
        /// <returns>Transa��o de saque</returns>
        internal Transaction Withdraw(decimal value)
        {
            value = Math.Round(value, 2);
            decimal newBalance = this.Balance - value;
            List<ValidationError> errors = new List<ValidationError>();

            if (value > MAX_VALUE)
            {
                errors.Add(new ValidationError($"O valor deve ser menor que {MAX_VALUE}.", "Value", "Transaction"));
            }

            if (value <= 0)
            {
                errors.Add(new ValidationError("O valor deve ser maior que zero."));
            }
            else if (newBalance < 0 && (-newBalance) > this.Account.OverdraftLimit)
            {
                errors.Add(new ValidationError("Sem fundos para realizar saque"));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException($"Erro ao realizar saque.", errors.ToArray());
            }

            this.NextTransaction = new Transaction(this.Account, DateTime.Now, TransactionType.Withdraw, -value, newBalance);
            return this.NextTransaction;
        }

        /// <summary>
        /// Dep�sito
        /// 
        /// Sendo esta a �ltima transa��o do dia do cliente
        /// Realiza o dep�sito, gerando uma nova transa��o e armazenando 
        /// o id da proxima transa��o nesta.
        /// </summary>
        /// <param name="value">Valor do dep�sito</param>
        /// <returns>Transa��o de dep�sito</returns>
        internal Transaction Deposit(decimal value)
        {
            List<ValidationError> errors = new List<ValidationError>();
            value = Math.Round(value, 2);
            if (value <= 0)
            {
                errors.Add(new ValidationError("O valor deve ser maior que zero.", "Value", "Transaction"));
            }

            if (value > MAX_VALUE)
            {
                errors.Add(new ValidationError($"O valor deve ser menor que {MAX_VALUE}.", "Value", "Transaction"));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException($"Erro ao realizar o dep�sito.", errors.ToArray());
            }
            this.NextTransaction = new Transaction(this.Account, DateTime.Now, TransactionType.Deposit, value, this.Balance + value);
            return this.NextTransaction;
        }

        /// <summary>
        /// Valida filtro da pesquisa de extrato
        /// </summary>
        /// <param name="start">data de inicio</param>
        /// <param name="end">data de fim</param>
        internal static void ValidateStatementFilter(ref DateTime start, ref DateTime? end)
        {
            List<ValidationError> errors = new List<ValidationError>();

            start = start.Date;
            end = end.GetValueOrDefault(DateTime.Now).Date.AddDays(1);

            if (start > DateTime.Now.Date)
            {
                errors.Add(new ValidationError("N�o pesquisar data futura.", "Start", "Transaction"));
            }

            if (end.Value < start)
            {
                errors.Add(new ValidationError("A data inicial n�o pode ser posterior a data final.", "Start", "Transaction"));
            }

            if ((end.Value - start).TotalDays > 100)
            {
                errors.Add(new ValidationError("A pesquisa n�o pode exceder 100 dias.", "Start", "Transaction"));
            }

            DateTime minDate = new DateTime(2020, 3, 1);
            if (start < minDate)
            {
                errors.Add(new ValidationError($"Data m�nima � {minDate:dd/MM/yyyy}.", "Start", "Transaction"));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Filtro de datas inv�lido.", errors.ToArray());
            }
        }
    }
}