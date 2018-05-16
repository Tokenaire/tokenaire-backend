using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using org.whispersystems.curve25519;
using RestSharp;
using Tokenaire.Database;
using Tokenaire.Database.Models;
using Tokenaire.Service.Enums;
using Tokenaire.Service.Models;
using Tokenaire.Service.Models.Models;
using WavesCS;

namespace Tokenaire.Service
{
    public interface IBitcoinService
    {
        string GenerateMultiSigAddressForIcoParticipant(int userId);
    }

    public class BitcoinService : IBitcoinService
    {
        public string GenerateMultiSigAddressForIcoParticipant(int userId)
        {
            var derivedExtPubKeys = new List<ExtPubKey>();
            var userIdSaltedSlightly = userId + 100;

            foreach (var encodedExtendedPubKey in new[] { "32", "3232", "3242" })
            {
                var extPubKey = new ExtPubKey(Encoders.Hex.DecodeData(encodedExtendedPubKey));
                var keyPath = new KeyPath($"{ServiceBitcoinDerivationPathEnum.Level1ICO}'/{userIdSaltedSlightly}");
                derivedExtPubKeys.Add(extPubKey.Derive(keyPath));
            }

            var redeemScript = PayToMultiSigTemplate
                .Instance
                .GenerateScriptPubKey(2, new[] {
                    derivedExtPubKeys[0].PubKey,
                    derivedExtPubKeys[1].PubKey,
                    derivedExtPubKeys[2].PubKey
                    });

            var multiSigAddress = redeemScript.Hash.GetAddress(Network.Main);


            /*

            ceoKey = new ExtKey();
            string accounting = "1'";
            int customerId = 5;
            int paymentId = 50;
            KeyPath path = new KeyPath(accounting + "/" + customerId + "/" + paymentId);
            //Path : "1'/5/50"
            ExtKey paymentKey = ceoKey.Derive(path);

             */


            return "";
        }
    }
}

