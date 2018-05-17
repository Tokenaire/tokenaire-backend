using System;
using System.Collections.Generic;
using System.IO;

namespace WavesCS
{
    public static class Utils
    {        
        public static void WriteLong(this BinaryWriter writer, long n)
        {
            n = System.Net.IPAddress.HostToNetworkOrder(n);
            writer.Write(n);
        }
        
        public static void WriteByte(this BinaryWriter writer, byte n)
        {            
            writer.Write(n);
        }
        
        public static void Write(this BinaryWriter writer, TransactionType n)
        {            
            writer.Write((byte) n);
        }

        public static void WriteShort(this BinaryWriter writer, int n)
        {
            byte[] shortN = BitConverter.GetBytes((short) n);
            Array.Reverse(shortN);
            writer.Write(shortN);          
        }

        public static long CurrentTimestamp()
        {
            long epochTicks = new DateTime(1970, 1, 1).Ticks;
            return (DateTime.UtcNow.Ticks - epochTicks) / (TimeSpan.TicksPerSecond / 1000);
        }
        
        public static long DateToTimestamp(this DateTime date)
        {
            return (date - new DateTime(1970, 1, 1)).Ticks / (TimeSpan.TicksPerSecond / 1000);             
        }

        public static void WriteAsset(this BinaryWriter stream, string assetId)
        {
            if (string.IsNullOrEmpty(assetId))
            {
                stream.WriteByte(0);
            }
            else
            {
                stream.WriteByte(1);
                var decoded = Base58.Decode(assetId);
                stream.Write(decoded, 0, decoded.Length);
            }
        }  
    }
}