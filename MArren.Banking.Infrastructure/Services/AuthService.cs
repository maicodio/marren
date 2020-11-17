using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Marren.Banking.Domain.Model;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Marren.Banking.Infrastructure.Services
{
    /// <summary>
    /// Serviço de autorização.
    /// 
    /// Gera o hash da senha para o controle de acesso de contas
    /// Gera o Token de acesso usando JwtSecurity
    /// </summary>
    public class AuthService : Marren.Banking.Domain.Contracts.IAuthService
    {
        /// <summary>
        /// Gera um hash para uma senha. 
        /// Usado para gravar o hash no banco de dados e para validar uma senha
        /// No nosso caso, utiliza SHA256
        /// </summary>
        /// <param name="password">Senha</param>
        /// <returns>Hash da senha</returns>
        public async Task<string> GenerateHash(string password)
        {
            const string saltguid = "82BF603B-5577-4C9E-9FE4-193A27F8D9DC";
            var sha = System.Security.Cryptography.SHA256.Create();
            var buffer = sha.ComputeHash(Encoding.UTF8.GetBytes(saltguid + password));
            return string.Join(null, buffer.Select(b => b.ToString("x2")));
        }

        /// <summary>
        /// Gera Token JWT
        /// </summary>
        /// <param name="account">Conta corrente autenticada</param>
        /// <returns></returns>
        public static string GenerateToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, account.Name),
                    new Claim("marren_account_id", account.Id.ToString()),
                    new Claim(ClaimTypes.Role, "client"),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(GetSecret()), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Chave privada para geração de tokens.
        /// </summary>
        /// <returns>Bytes do token</returns>
        public static byte[] GetSecret()
        {
            return Encoding.ASCII.GetBytes("C636A24A-95A3-46BC-8BB9-B8ECEA9B298D");
        }
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously