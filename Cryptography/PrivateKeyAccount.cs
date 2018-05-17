using System.Text;
using System;
using System.Linq;
using System.IO;
using org.whispersystems.curve25519.csharp;
using System.Security.Cryptography;
using System.Numerics;
using System.Collections.Generic;
using org.whispersystems.curve25519;

namespace WavesCS
{
    public class PrivateKeyAccount
    {
        private static readonly SHA256Managed SHA256 = new SHA256Managed();

        private static readonly Curve25519 Cipher = Curve25519.getInstance(Curve25519.BEST);
                
        private readonly byte[] _privateKey;
        private readonly byte[] _publicKey;

        public byte[] PrivateKey => _privateKey.ToArray();
        public byte[] PublicKey => _publicKey.ToArray();

        private PrivateKeyAccount(byte[] privateKey, char scheme)         
        {
            _publicKey = GetPublicKeyFromPrivateKey(privateKey);
            _privateKey = privateKey;
        }

        private PrivateKeyAccount(string privateKey, char scheme) : this(Base58.Decode(privateKey), scheme) { }

        public static PrivateKeyAccount CreateFromPrivateKey(string privateKey, char scheme)
        {
            return new PrivateKeyAccount(privateKey, scheme);
        }        

        private static byte[] GetPublicKeyFromPrivateKey(byte[] privateKey)
        {
            var publicKey = new byte[privateKey.Length];
            Curve_sigs.curve25519_keygen(publicKey, privateKey);
            return publicKey;
        }        

        public string Sign(MemoryStream stream)
        {
            var bytesToSign = stream.ToArray();
            var signature = Cipher.calculateSignature(PrivateKey, bytesToSign);
            return Base58.Encode(signature);
        }
    }
}