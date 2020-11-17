using System;

namespace Marren.Banking.Domain.Kernel
{
    /// <summary>
    /// Classe para indicar um problema de valida��o no dom�nio
    /// </summary>
    public class ValidationError
    {   
        /// <summary>
        /// A entidade origem do dom�nio
        /// </summary>
        public string Source { get; private set; }
        
        /// <summary>
        /// Um identificador do campo que originou o problema
        /// </summary>
        public string Id { get; private set; } 

        /// <summary>
        /// Descri��o do problema
        /// </summary>
        public string Message { get; private set; } 

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="message">Descri��o do problema</param>
        /// <param name="id">Identificador do campo relacionado</param>
        /// <param name="source">Entidade que originou o problema</param>
        public ValidationError(string message, string id=null, string source=null)
        {
            this.Message = message;
            this.Id = id;
            this.Source = source;
        }
    }
}