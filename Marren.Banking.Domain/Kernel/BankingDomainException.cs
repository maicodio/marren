using System;
using System.Collections.Generic;
using System.Linq;

namespace Marren.Banking.Domain.Kernel
{
    /// <summary>
    /// Exceção padrão emitida nas validações das regras do domínio
    /// </summary>
    public class BankingDomainException : ApplicationException
    {
        /// <summary>
        /// Lista de erros da validação
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
        /// <param name="message">mensagem de erro mais genérica</param>
        /// <param name="errors">Lista de erros de validação</param>
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
        /// Construtor para tratamento de outras exceções.
        /// </summary>
        /// <param name="message">mensagem do erro</param>
        /// <param name="innerException">Exceção tratada.</param>
        public BankingDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}