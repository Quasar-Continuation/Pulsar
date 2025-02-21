using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Quasar.Server.Helpers
{
    // Token: 0x02000029 RID: 41
    public static class Zip
    {
        // Token: 0x060002A8 RID: 680 RVA: 0x00009460 File Offset: 0x00007660
        public static byte[] Decompress(byte[] input)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream(input))
            {
                byte[] array = new byte[4];
                memoryStream.Read(array, 0, 4);
                int num = BitConverter.ToInt32(array, 0);
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    byte[] array2 = new byte[num];
                    gzipStream.Read(array2, 0, num);
                    result = array2;
                }
            }
            return result;
        }

        // Token: 0x060002A9 RID: 681 RVA: 0x000094E0 File Offset: 0x000076E0
        public static byte[] Compress(byte[] input)
        {
            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = BitConverter.GetBytes(input.Length);
                memoryStream.Write(bytes, 0, 4);
                using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(input, 0, input.Length);
                    gzipStream.Flush();
                }
                result = memoryStream.ToArray();
            }
            return result;
        }

        // Token: 0x060002AA RID: 682 RVA: 0x0000955C File Offset: 0x0000775C
        public static string GetChecksum(string file)
        {
            string result;
            using (FileStream fileStream = File.OpenRead(file))
            {
                result = BitConverter.ToString(new SHA256Managed().ComputeHash(fileStream)).Replace("-", string.Empty);
            }
            return result;
        }
    }
}
