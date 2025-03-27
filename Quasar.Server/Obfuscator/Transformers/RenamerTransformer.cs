using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quasar.Obfuscator.Transformers
{
    public class RenamerTransformer : ITransformer
    {
        
        public override void Transform(Obfuscator obf)
        {
            //TODO: rewrite this and use this transformer instead of the Build/Renamer one

            //ModuleDefMD module = obf.Module;


            //module.Name = RandomString(10);
            //module.Assembly.Name = RandomString(10);
            //module.Assembly.Culture = RandomString(10);
            //module.Assembly.Version = new Version(random.Next(0, 10), random.Next(0, 10), random.Next(0, 10), random.Next(0, 10));

            //foreach (TypeDef type in module.GetTypes())
            //{
            //    if (type.IsRuntimeSpecialName) continue;
            //    if (type.IsSpecialName) continue;
            //    if (type.IsGlobalModuleType) continue;
            //    if (type.IsWindowsRuntime) continue;
            //    if (type.IsInterface) continue;

            //    type.Namespace = "";
            //    type.Name = RandomUTFString(10);

            //    foreach (PropertyDef property in type.Properties)
            //    {
            //        property.Name = RandomUTFString(10);
            //    }

            //    foreach (FieldDef field in type.Fields)
            //    {
            //        field.Name = RandomUTFString(10);
            //    }

            //    foreach (EventDef eventDef in type.Events) { 
            //        eventDef.Name = RandomUTFString(10);
            //    }

            //    foreach (MethodDef method in type.Methods)
            //    {

            //       if(!method.HasBody) continue;

            //        foreach (ParamDef param in method.ParamDefs)
            //        {
            //            param.Name = RandomUTFString(10);
            //        }

            //        foreach (Local local in method.Body.Variables)
            //        {
            //            local.Name = RandomUTFString(10);
            //        }

            //    }

            //}
        }
    }
    
}
