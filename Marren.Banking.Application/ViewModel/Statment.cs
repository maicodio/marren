using System;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// View Model do Extrato
    /// </summary>
    public class Statement
    {
        /// <summary>Data do movimento</summary>
        public DateTime Date { get; set; }
        /// <summary>Tipo</summary>
        public string Type { get; set; }
        /// <summary>Valor</summary>
        public decimal Value { get; set; }
        /// <summary>Saldo</summary>
        public decimal Balance { get; set; }
    }
}
