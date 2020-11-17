using System;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// ModelView para criação de conta corrente
    /// </summary>
    public class CreateAccount
    {
        /// <summary>
        /// Nome do titular
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Senha
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// LImite Cheque especial
        /// </summary>
        public decimal OverdraftLimit { get; set; }

        /// <summary>
        /// Taxa do cheque especial
        /// </summary>
        public decimal OverdraftTax { get; set; }

        /// <summary>
        /// Depósito inicial
        /// </summary>
        public decimal initialDeposit { get; set; }

        /// <summary>
        /// Data de abertura
        /// </summary>
        public DateTime OpeningDate { get; set; }
    }
}
