using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Obfuscator.Core.Transformers
{
    public class ITransformer
    {

        protected static readonly Random random = new Random();

        public virtual void Transform(Obfuscator obf) { }

        protected string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                             .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected string RandomUTFString(int length)
        {
            // make weird utf strings like \u0x200 etc
            return new string(Enumerable.Repeat(0, length)
                                            .Select(s => (char)random.Next(0, 0xFFFF)).ToArray());
        }

    }
}
