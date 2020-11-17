using System;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Contracts
{
    /// <summary>
    /// Contrato para o serviço de autorização.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Gera um hash para uma senha. 
        /// Usado para gravar o hash no banco de dados e para validar uma senha
        /// </summary>
        /// <param name="password">Senha</param>
        /// <returns>Hash da senha</returns>
        Task<string> GenerateHash(string password);
    }
}