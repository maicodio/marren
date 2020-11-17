using System;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// ViewModel do Saque
    /// </summary>
    public class Withdraw
    {
        /// <summary>Senha</summary>
        public string Password { get; set; }

        /// <summary>Valor</summary>
        public decimal Ammount { get; set; }
    }
}
