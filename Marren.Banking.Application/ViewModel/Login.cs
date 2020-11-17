using System;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// ModelView do login
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Numero da conta
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Senha
        /// </summary>
        public string Password { get; set; }
    }
}
