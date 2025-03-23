using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Quasar.Obfuscator.Core.Utils;
using Quasar.Obfuscator.Core.Utils.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Obfuscator.Core.Transformers
{
    public class StringEncryptionTransformer : ITransformer
    {


        public MethodDef InjectDecryptionMethod(ModuleDefMD module)
        {
            // create our type holding our string decryption method
            TypeDef stringType = new TypeDefUser("", RandomUTFString(10), module.CorLibTypes.Object.TypeDefOrRef);
            module.Types.Add(stringType);
            
            // inject the methods we want to it

            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(StringEncryption).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(StringEncryption).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, stringType, module);
            MethodDef decryptMethod = (MethodDef)members.Single(method => method.Name == "Decrypt");
            decryptMethod.Name = RandomUTFString(10);


            // remove the Encrypt method
            stringType.Methods.Remove(stringType.Methods.Single(method => method.Name == "Encrypt")); 

            return decryptMethod;
        }

        public override void Transform(Obfuscator obf)
        {
            // Inject the Decrypt method into the type
            MethodDef decryptMethod = InjectDecryptionMethod(obf.Module);

            foreach (TypeDef type in obf.Module.GetTypes())
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    method.Body.SimplifyBranches();


                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        if (method.Body.Instructions[i].OpCode == dnlib.DotNet.Emit.OpCodes.Ldstr)
                        {
                            object op = method.Body.Instructions[i].Operand;
                            if (op is string)
                            {
                                AesManaged aes = new AesManaged();
                                string key = Convert.ToBase64String(aes.Key);
                                string iv = Convert.ToBase64String(aes.IV);

                                string encrypted = StringEncryption.Encrypt((string)op, aes.Key, aes.IV);

                                // Call to decrypt(value, key, iv);
                                method.Body.Instructions[i].Operand = encrypted;
                                method.Body.Instructions.Insert(i + 1, OpCodes.Ldstr.ToInstruction(key));
                                method.Body.Instructions.Insert(i + 2, OpCodes.Ldstr.ToInstruction(iv));
                                method.Body.Instructions.Insert(i + 3, OpCodes.Call.ToInstruction(decryptMethod));

                               

                                i += 3; // skip the added instructions

                            }
                        }
                    }
                }
            }
        }
    }
}
