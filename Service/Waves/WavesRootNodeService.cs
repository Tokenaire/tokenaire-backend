using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Models;

namespace Tokenaire.Service
{
    public interface IWavesRootNodeService
    {
        IWavesAddressesNodeService Addresses {get;}
    }

    public class WavesRootNodeService : IWavesRootNodeService
    {
        public IWavesAddressesNodeService Addresses { get; }

        public WavesRootNodeService(IWavesAddressesNodeService wavesAddressesNodeService)
        {
            Addresses = wavesAddressesNodeService;
        }
    }
}

