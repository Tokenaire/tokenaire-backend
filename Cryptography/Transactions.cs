using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using DictionaryObject = System.Collections.Generic.Dictionary<string, object>;

namespace WavesCS
{    

    public enum TransactionType : byte
    {
        Issue = 3,
        Transfer = 4,
        Reissue = 5,
        Burn = 6,
        Lease = 8,
        LeaseCancel = 9,
        Alias = 10,
        DataTx = 12,    
    }
    
    public static class Transactions
    {
        private static readonly int MinBufferSize = 300;

        public static DictionaryObject MakeTransferTransaction(PrivateKeyAccount account, string toAddress,
            long amount, string assetId, long fee, string feeAssetId, string attachment)
        {

            byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachment ?? "");
            long timestamp = Utils.CurrentTimestamp();

            var stream = new MemoryStream(MinBufferSize);
            var writer = new BinaryWriter(stream);
            writer.Write(TransactionType.Transfer);
            writer.Write(account.PublicKey);
            writer.WriteAsset(assetId);
            writer.WriteAsset(feeAssetId);
            writer.WriteLong(timestamp);
            writer.WriteLong(amount);
            writer.WriteLong(fee);
            writer.Write(Base58.Decode(toAddress));
            //writer.Write((short)attachmentBytes.Length);
            writer.WriteShort((short) attachmentBytes.Length);
            writer.Write(attachmentBytes);
            string signature = account.Sign(stream);
            return new DictionaryObject
            {
                {"type", TransactionType.Transfer},
                {"senderPublicKey", Base58.Encode(account.PublicKey)},
                {"signature", signature},
                {"recipient", toAddress},
                {"amount", amount},
                {"assetId", assetId},
                {"fee", fee},
                {"feeAssetId", feeAssetId},
                {"timestamp", timestamp},
                {"attachment", Base58.Encode(attachmentBytes)}
            };
        }
    }
}