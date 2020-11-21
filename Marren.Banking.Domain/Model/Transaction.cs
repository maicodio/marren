using Marren.Banking.Domain.Kernel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Marren.Banking.Domain.Model
{
    /// <summary>
    /// Entidade que representa um movimento da conta corrente
    /// </summary>
    public class Transaction : Entity
    {
        /// <summary>
        /// Valor máximo de uma transação (movimento)
        /// </summary>
        private const decimal MAX_VALUE = 1000000000m;

        /// <summary>
        /// Conta corrente vinculada
        /// </summary>
        public Account Account { get; private set; }

        /// <summary>
        /// Data e hora da transação (movimento)
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Tipo de transação
        /// </summary>
        public TransactionType Type { get; private set; }

        /// <summary>
        /// Valor da transação.
        /// Pode ser negativo, de acordo com o tipo.
        /// </summary>
        public decimal Value { get; private set; }

        /// <summary>
        /// Saldo da conta.
        /// </summary>
        public decimal Balance { get; private set; }

        /// <summary>
        /// Armazena o ID da proxima transação.
        /// </summary>
        public Transaction NextTransaction { get; private set; }

        /// <summary>
        /// Armazena uma referência à transação.
        /// Ex: Conta origem ou destino de uma transferência
        /// </summary>
        public string Reference { get; private set; }

        /// <summary>
        /// Construtor interno
        /// </summary>
        protected Transaction()
        { }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="account">Conta corrente</param>
        /// <param name="date">Data da operação</param>
        /// <param name="type">Tipo de transação</param>
        /// <param name="value">Valor</param>
        /// <param name="balance">Saldo</param>
        /// <param name="nextTransaction">Próxima transação</param>
        /// <param name="reference">Referência</param>
        public Transaction(Account account, DateTime date, TransactionType type, decimal value, decimal balance, Transaction nextTransaction = null, string reference = null) : this()
        {
            var errors = new List<ValidationError>();

            this.Account = account;
            this.Date = date;
            this.Type = type;
            this.Value = value;
            this.Balance = balance;
            this.NextTransaction = nextTransaction;
            this.Reference = reference;

            this.Validate(errors);

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Campos inválidos", errors.ToArray());
            }
        }

        /// <summary>
        /// Valida a transação
        /// </summary>
        /// <param name="errors">Lista de erros</param>
        private void Validate(IList<ValidationError> errors)
        {
            if (this.Account == null)
            {
                errors.Add(new ValidationError("Conta inválida", "Account", "Transaction"));
            }

            if (this.Date > DateTime.Now)
            {
                errors.Add(new ValidationError("Data da transferência inválida.", "Value", "Transaction"));
            }

            if (this.Type == null)
            {
                errors.Add(new ValidationError("Tipo de transferência não informado.", "Type", "Transaction"));
            }

            if (!Enumeration.GetAll<TransactionType>().Any(x => x.Id == this.Type.Id))
            {
                errors.Add(new ValidationError("Tipo de transferência inválido", "Type", "Transaction"));
            }

            this.Value = Math.Round(this.Value, 2);
            this.Balance = Math.Round(this.Balance, 2);
        }

        /// <summary>
        /// Método para calcular o saldo do dia seguinte
        /// 
        /// O saldo da conta do cliente fica armazenado na última transação realizada pelo cliente.
        /// 
        /// Quando se busca o saldo atualizado é necessário processar todos os dias desde a última transação
        /// para calcular rendimentos e taxas da conta do cliente.
        /// 
        /// Sendo esta transação a última transação da conta, e não sendo do dia de hoje, este método calcula o saldo
        /// do dia seguinte pelos parâmetros, gerando um movimento de saldo e um movimento de rendimento ou taxa.
        /// 
        /// O método AccountService GetLastTransaction orquestra a obtenção desse saldo, chamando este método enquanto
        /// não estiver com o saldo do dia.
        /// </summary>
        /// <param name="interestRate">Taxa de juros do dia</param>
        /// <param name="overdraftTax">Taxa do limite do cheque especial</param>
        /// <returns>Lista de transações do saldo do dia seguinte</returns>
        internal IEnumerable<Transaction> GenerateNextDayBalance(decimal interestRate, decimal overdraftTax)
        {
            DateTime newDate = this.Date.Date.AddDays(1);
            List<ValidationError> errors = new List<ValidationError>();

            if (this.NextTransaction != null)
            {
                errors.Add(new ValidationError("Está não é a última transação."));
            }

            if (interestRate < 0)
            {
                errors.Add(new ValidationError("Taxa de juros inválida."));
            }

            if (overdraftTax < 0)
            {
                errors.Add(new ValidationError("Taxa do cheque especial inválida."));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException($"Erro ao processar taxas da conta {this.Account.Id} para o dia {newDate:yyyy-MM-dd}", errors.ToArray());

            }

            Transaction tax = null;
            //interestRate == 0 significa que era feriado. Não calcula juros nem taxas.
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
        /// Sendo esta a ultima transação do cliente contendo o saldo:
        /// Gera os movimentos de transferênica de conta
        /// </summary>
        /// <param name="value">Valor</param>
        /// <param name="lastTransactionDeposit">Ultima transação da conta de deposito</param>
        /// <returns>As duas trasações da transferencia, uma de saida e outra de entrada</returns>
        internal IEnumerable<Transaction> Transfer(decimal value, Transaction lastTransactionDeposit)
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
                errors.Add(new ValidationError("O valor deve ser maior que zero.", "Value", "Transaction"));
            }
            else if (newBalance < 0 && (-newBalance) > this.Account.OverdraftLimit)
            {
                errors.Add(new ValidationError("Sem fundos para realizar a transferência", "Value", "Transaction"));
            }

            if (lastTransactionDeposit.Account == this.Account)
            {
                errors.Add(new ValidationError("Transferências para a mesma conta não são permitidas", "AccountId", "Transaction"));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException($"Erro ao realizar a transferência.", errors.ToArray());
            }

            this.NextTransaction = new Transaction(
                this.Account, DateTime.Now, TransactionType.TransferOut, -value, newBalance, null, lastTransactionDeposit.Account.Id.ToString());
            
            yield return this.NextTransaction;

            decimal newBalanceDeposit = lastTransactionDeposit.Balance + value;

            lastTransactionDeposit.NextTransaction = new Transaction(
                lastTransactionDeposit.Account, DateTime.Now, TransactionType.TransferIn, value, newBalanceDeposit, null, this.Account.Id.ToString());

            yield return lastTransactionDeposit.NextTransaction;
        }

        /// <summary>
        /// Saque
        /// 
        /// Sendo esta a última transação do dia do cliente
        /// Realiza o saque, gerando uma nova transação e armazenando 
        /// o id da proxima transação nesta.
        /// </summary>
        /// <param name="value">Valor a ser sacado</param>
        /// <returns>Transação de saque</returns>
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
        /// Depósito
        /// 
        /// Sendo esta a última transação do dia do cliente
        /// Realiza o depósito, gerando uma nova transação e armazenando 
        /// o id da proxima transação nesta.
        /// </summary>
        /// <param name="value">Valor do depósito</param>
        /// <returns>Transação de depósito</returns>
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
                throw new BankingDomainException($"Erro ao realizar o depósito.", errors.ToArray());
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
                errors.Add(new ValidationError("Não pesquisar data futura.", "Start", "Transaction"));
            }

            if (end.Value < start)
            {
                errors.Add(new ValidationError("A data inicial não pode ser posterior a data final.", "Start", "Transaction"));
            }

            if ((end.Value - start).TotalDays > 100)
            {
                errors.Add(new ValidationError("A pesquisa não pode exceder 100 dias.", "Start", "Transaction"));
            }

            DateTime minDate = new DateTime(2020, 3, 1);
            if (start < minDate)
            {
                errors.Add(new ValidationError($"Data mínima é {minDate:dd/MM/yyyy}.", "Start", "Transaction"));
            }

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Filtro de datas inválido.", errors.ToArray());
            }
        }
    }
}