using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace Marren.Banking.Infrastructure.Services
{
    /// <summary>
    /// Serviço para obtenção de parametros financeiros.
    /// 
    /// Fornece taxa selic para o sistema de conta corrente.
    /// </summary>
    public class FinanceService : Marren.Banking.Domain.Contracts.IFinanceService
    {
        /// <summary>
        /// Busca a taxa de juros para calculo de taxas e juros.
        /// Deve retornar registros apenas para dias úteis bancários.
        /// 
        /// Este serviço busca as taxas e datas bancárias úteis do BACEN.
        /// Taxa SELIC
        /// </summary>
        /// <param name="start">Data ínicio da pesquisa</param>
        /// <param name="end">Data fim da pesquisa</param>
        /// <returns>Asyncronamente, retorna uma lista de datas e a respectiva taxa de juros apurada no dia.</returns>
        public async Task<Dictionary<string, decimal>> GetInterestRate(DateTime start, DateTime end)
        {
            try
            {
                string url = $"https://api.bcb.gov.br/dados/serie/bcdata.sgs.11/dados?formato=json&dataInicial={start:dd/MM/yyyy}&dataFinal={end:dd/MM/yyyy}";

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9");
                client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("Host", "api.bcb.gov.br");
                client.DefaultRequestHeaders.Add("If-None-Match", "W/\"53 - ObMpbapUebZdEez8mvJxYQ\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"86\", \"\\\"Not\\\\A;Brand\"; v = \"99\", \"Google Chrome\"; v = \"86\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.193 Safari/537.36");

                string dataTxt = await client.GetStringAsync(url);
                var dataJson = System.Text.Json.JsonDocument.Parse(dataTxt);
                Dictionary<string, decimal> result = new Dictionary<string, decimal>();
                var enUs = new System.Globalization.CultureInfo("en-US");
                foreach (var item in dataJson.RootElement.EnumerateArray())
                {
                    result.Add(
                        DateTime.ParseExact(item.GetProperty("data").GetString(), "dd/MM/yyyy", null).ToString("yyyyMMdd"), 
                        Decimal.Parse(item.GetProperty("valor").GetString(), enUs)/100);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Banking.Domain.Kernel.BankingDomainException("Problema ao carregar selic", ex);
            }
        }
    }
}
