using System;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// ModelView do resultado de sucesso do login
    /// </summary>
    public class AccountToken
    {

        /// <summary>
        /// Id da conta
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Nome
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}
