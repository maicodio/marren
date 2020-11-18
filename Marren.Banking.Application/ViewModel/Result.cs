using Marren.Banking.Domain.Kernel;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Marren.Banking.Application.ViewModel
{
    /// <summary>
    /// ViewModel de resultado padrão da API
    /// </summary>
    public class Result
    {
        /// <summary>Ok = Sucesso</summary>
        public bool Ok { get; private set; }

        /// <summary>Mensagem</summary>
        public string Message { get; private set; }

        /// <summary>Erros</summary>
        public IReadOnlyCollection<ValidationError> Errors { get; private set; }
        
        /// <summary>Construtor</summary>
        public Result()
        { }

        /// <summary>Construtor por exception</summary>
        public Result(BankingDomainException ex)
        {
            this.Ok = false;
            this.Message = ex.Message;
            this.Errors = ex.ValidationErrors;
        }

        /// <summary>Construtor com mensagem</summary>
        public Result(bool ok, string message)
        {
            this.Ok = ok;
            this.Message = message;
        }

        /// <summary>
        /// Sugar construtor
        /// </summary>
        /// <typeparam name="T">Tipo de result</typeparam>
        /// <param name="ex">Exception</param>
        /// <returns>Result View Model</returns>
        public static Result<T> Create<T>(BankingDomainException ex)
        {
            return new Result<T>(ex);
        }

        /// <summary>
        /// Sugar constructor
        /// </summary>
        /// <typeparam name="T">Tipo de dados do retorno</typeparam>
        /// <param name="data">Dados</param>
        /// <param name="message">Mensagem</param>
        /// <returns>Result View Model</returns>
        public static Result<T> Create<T>(T data, string message=null)
        {
            return new Result<T>(data, message);
        }

        /// <summary>
        /// Sugar Constructor
        /// </summary>
        /// <typeparam name="T">Tipo de dados do retorno</typeparam>
        /// <param name="message">Mensagem de errro</param>
        /// <returns>Result View Model</returns>
        public static Result<T> CreateError<T>(string message)
        {
            return new Result<T>(message);
        }
    }

    /// <summary>
    /// ViewModel de resultado padrão da API
    /// Com dados genéricos
    /// </summary>
    /// <typeparam name="T">Tipo de dado retornado</typeparam>
    public class Result<T> : Result
    {
        /// <summary>Dados retornados</summary>
        public T Data { get; private set; }

        /// <summary>Constructor com base em erro</summary>
        public Result(BankingDomainException ex) : base(ex)
        { }

        /// <summary>Constructor com dados</summary>
        public Result(T data, string message=null): base(true, message)
        {
            this.Data = data;
        }

        /// <summary>Constructor com mensagem</summary>
        public Result(string message) : base(false, message)
        { }
    }
}
