using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Marren.Banking.Domain.Kernel;

[assembly: InternalsVisibleTo("Marren.Banking.Tests")]
namespace Marren.Banking.Domain.Model 
{
    /// <summary>
    /// Entidade que representa uma conta corrente
    /// </summary>
    public class Account: Entity
    {
        /// <summary>
        /// Nome da conta
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Data de abertura da conta
        /// </summary>
        public DateTime OpeningDate { get; private set; }

        /// <summary>
        /// Limite do cheque especial
        /// </summary>
        public decimal OverdraftLimit { get; private set; }

        /// <summary>
        /// Taxa do cheque especial
        /// </summary>
        public decimal OverdraftTax { get; private set; }

        /// <summary>
        /// Hash da senha da conta
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Construtor interno
        /// </summary>
        protected Account()
        { }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name">Nome da conta</param>
        /// <param name="overdraftLimit">Limite do cheque especial</param>
        /// <param name="overdraftTax">Taxa do cheque especial</param>
        /// <param name="password">Senha (para fins de validação)</param>
        /// <param name="passwordHash">Hash da senha</param>
        /// <param name="openingDate">Data de abertura da conta</param>
        public Account(string name, decimal overdraftLimit, decimal overdraftTax, string password, string passwordHash, DateTime openingDate): this()
        {
            var errors = new List<ValidationError>();

            this.OpeningDate = openingDate;
            this.Name = name;
            this.OverdraftLimit = overdraftLimit;
            this.OverdraftTax = overdraftTax;
            this.PasswordHash = passwordHash;

            this.Validate(errors);

            Account.ValidatePassword(password, errors);

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Campos inválidos", errors.ToArray());
            }
        }

        /// <summary>
        /// Validação interna
        /// </summary>
        /// <param name="errors">Erros encontrados na validação</param>
        private void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                errors.Add(new ValidationError("Campo Requerido", "Name", "Account"));
            }
            else
            {
                this.Name = this.Name.Trim();

                if (this.Name.Length > 50)
                {
                    errors.Add(new ValidationError("Nome tem mais que 50 caracteres.", "Name", "Account"));
                }
            }

            if (this.OverdraftLimit < 0 || this.OverdraftLimit > 1000000000)
            {
                errors.Add(new ValidationError("Valor deve estar entre  e 1000000000.", "OverdraftLimit", "Account"));
            }

            if (this.OverdraftTax < 0 || this.OverdraftTax > 1)
            {
                errors.Add(new ValidationError("Valor deve estar entre 0 e 1.", "OverdraftTax", "Account"));
            }

            if (string.IsNullOrWhiteSpace(this.PasswordHash))
            {
                errors.Add(new ValidationError("Campo Requerido.", "Name", "Account"));
            }

            if (this.OpeningDate > DateTime.Now)
            {
                errors.Add(new ValidationError("Data futura não permitida.", "OpeningDate", "Account"));
            }

            DateTime minDate = new DateTime(2020, 3, 1);
            if (this.OpeningDate < minDate)
            {
                errors.Add(new ValidationError($"Data mínima é {minDate:dd/MM/yyyy}.", "OpeningDate", "Account"));
            }
        }

        /// <summary>
        /// Valida a senha da conta.
        /// 
        /// É estático porque é exposto para que possa ser usada em serviços.
        /// </summary>
        /// <param name="password">Senha</param>
        /// <param name="errors">Lista de erros</param>
        private static void ValidatePassword(string password, List<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add(new ValidationError("Campo Senha inválido.", "Password", "Account"));
            }
            else if (password.Length < 3)
            {
                errors.Add(new ValidationError("Tamanho mínimo da senha é 3.", "Password", "Account"));
            }
        }

        /// <summary>
        /// Valida a senha da conta
        /// </summary>
        /// <param name="password">Senha</param>
        public static void ValidatePassword(string password)
        {
            var errors = new List<ValidationError>();

            ValidatePassword(password, errors);

            if (errors.Count > 0)
            {
                throw new BankingDomainException("Campos inválidos.", errors.ToArray());
            }
        }
    }
}