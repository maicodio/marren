using System;
using System.Collections.Generic;
using System.Linq;

namespace Marren.Banking.Domain.Kernel
{
    /// <summary>
    /// Exce��o padr�o emitida nas valida��es das regras do dom�nio
    /// </summary>
    public class BankingDomainException : ApplicationException
    {
        /// <summary>
        /// Lista de erros da valida��o
        /// </summary>
        public IReadOnlyCollection<ValidationError> ValidationErrors { get; private set; } = new List<ValidationError>(); 

        /// <summary>
        /// Construtor vazio
        /// </summary>
        public BankingDomainException()
        { }

        /// <summary>
        /// Costrutor parametrizado
        /// </summary>
        /// <param name="message">mensagem de erro mais gen�rica</param>
        /// <param name="errors">Lista de erros de valida��o</param>
        public BankingDomainException(string message, params ValidationError[] errors)
            : base(message)
        { 
            this.ValidationErrors = errors.ToList();
        }

        /// <summary>
        /// Construtor para mensagem simples
        /// </summary>
        /// <param name="message">mensagem do erro</param>
        public BankingDomainException(string message)
            : base(message)
        { }

        /// <summary>
        /// Construtor para tratamento de outras exce��es.
        /// </summary>
        /// <param name="message">mensagem do erro</param>
        /// <param name="innerException">Exce��o tratada.</param>
        public BankingDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}