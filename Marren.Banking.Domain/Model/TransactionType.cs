using System;
using System.Linq;
using Marren.Banking.Domain.Kernel;

namespace Marren.Banking.Domain.Model 
{
    /// <summary>
    /// Tabela de tipos de transação
    /// </summary>
    public class TransactionType: Enumeration
    {
        /// <summary>
        /// Abertura de conta. 
        /// Transação marca o ínicio das operações.
        /// </summary>
        public static TransactionType Openning = new TransactionType(1, "Abertura");
        /// <summary>
        /// Saldo;
        /// Transação para marcar o saldo inicial de cada dia da conta
        /// </summary>
        public static TransactionType Balance = new TransactionType(2, "Saldo");
        /// <summary>
        /// Saque.
        /// Transação de saque.
        /// </summary>
        public static TransactionType Withdraw = new TransactionType(3, "Saque");
        /// <summary>
        /// Depósito;
        /// Transação de depósito
        /// </summary>
        public static TransactionType Deposit = new TransactionType(4, "Depósito");
        /// <summary>
        /// Rendimento.
        /// Transação que indica os rendimentos do dia anterior
        /// </summary>
        public static TransactionType Interest = new TransactionType(5, "Rendimento");
        /// <summary>
        /// Taxas.
        /// Transação de cobrança de taxa de uso do cheque especial
        /// </summary>
        public static TransactionType Fees = new TransactionType(6, "Taxas");
        /// <summary>
        /// Transferência entre contas 
        /// Transação de transferência de saída da conta
        /// </summary>
        public static TransactionType TransferOut = new TransactionType(7, "Pago para");

        /// <summary>
        /// Transferência entre contas
        /// Transação de transferência de entrada da conta
        /// </summary>
        public static TransactionType TransferIn = new TransactionType(8, "Recebido de");

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        internal TransactionType(int id, string name)
            : base(id, name)
        {
        }

    }
}