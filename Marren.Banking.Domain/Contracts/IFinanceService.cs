using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marren.Banking.Domain.Contracts
{
    /// <summary>
    /// Contrato dos servi�os financeiros
    /// </summary>
    public interface IFinanceService
    {
        /// <summary>
        /// Busca a taxa de juros para calculo de taxas e juros.
        /// Deve retornar registros apenas para dias �teis banc�rios.
        /// </summary>
        /// <param name="start">Data �nicio da pesquisa</param>
        /// <param name="end">Data fim da pesquisa</param>
        /// <returns>Asyncronamente, retorna uma lista de datas e a respectiva taxa de juros apurada no dia.</returns>
        Task<Dictionary<string, decimal>> GetInterestRate(DateTime start, DateTime end);
    }
}