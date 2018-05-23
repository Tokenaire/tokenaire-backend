using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RestSharp;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;
using tokenaire_backend.Extensions;

namespace Tokenaire.Service
{
    public interface IWavesCoinomatService
    {
        Task<string> GenerateBTCAddressFromWavesAddress(string wavesAddress);
    }

    public class WavesCoinomatService : IWavesCoinomatService
    {
        private readonly string coinomatApiUrl;

        public WavesCoinomatService(IConfiguration configuration)
        {
            coinomatApiUrl = configuration.GetValue<string>("CoinomatApiUrl");
        }

        public async Task<string> GenerateBTCAddressFromWavesAddress(string wavesAddress)
        {
            if (string.IsNullOrEmpty(wavesAddress)) {
                return null;
            }

            var clientCreateTunnel = new RestClient($"{coinomatApiUrl}/create_tunnel.php");
            var reqCreateTunnel = new RestRequest(Method.GET);

            reqCreateTunnel.AddQueryParameter("currency_from", "BTC");
            reqCreateTunnel.AddQueryParameter("currency_to", "WAVES");
            reqCreateTunnel.AddQueryParameter("wallet_to", wavesAddress);

            var respCreateTunnel = await clientCreateTunnel.GetAsyncReal(reqCreateTunnel);

            if (respCreateTunnel.StatusCode == HttpStatusCode.OK)
            {
                dynamic respContentCreateTunnel = JsonConvert.DeserializeObject(respCreateTunnel.Content);

                var tunnelId = Convert.ToString(respContentCreateTunnel.tunnel_id);
                var k1 = Convert.ToString(respContentCreateTunnel.k1);
                var k2 = Convert.ToString(respContentCreateTunnel.k2);

                var clientGetTunnel = new RestClient($"{coinomatApiUrl}/get_tunnel.php");
                var reqGetTunnel = new RestRequest("get_tunnel.php", Method.GET);
                reqGetTunnel.AddQueryParameter("xt_id", tunnelId);
                reqGetTunnel.AddQueryParameter("k1", k1);
                reqGetTunnel.AddQueryParameter("k2", k2);

                var respGetTunnel = await clientGetTunnel.GetAsyncReal(reqGetTunnel);
                if (respGetTunnel.StatusCode == HttpStatusCode.OK)
                {
                    dynamic respContentGetTunnel = JsonConvert.DeserializeObject(respGetTunnel.Content);
                    return respContentGetTunnel.tunnel.wallet_from;
                }
            }

            return null;
        }
    }
}

