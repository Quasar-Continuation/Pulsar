using Mono.Cecil;
using Quasar.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quasar.Server.Build
{
    public class Renamer
    {
        public AssemblyDefinition AsmDef { get; set; }

        private int MinLength { get; set; }
        private int MaxLength { get; set; }
        private MemberOverloader _typeOverloader;
        private Dictionary<TypeDefinition, MemberOverloader> _methodOverloaders;
        private Dictionary<TypeDefinition, MemberOverloader> _fieldOverloaders;
        private Dictionary<TypeDefinition, MemberOverloader> _eventOverloaders;
        private Dictionary<string, string> _namespaceRenames;

        private readonly SafeRandom _random = new SafeRandom();

        public Renamer(AssemblyDefinition asmDef)
            : this(asmDef, 10, 30)
        {
        }

        public Renamer(AssemblyDefinition asmDef, int minLength, int maxLength)
        {
            this.AsmDef = asmDef;
            this.MinLength = minLength;
            this.MaxLength = maxLength;
            _typeOverloader = new MemberOverloader(this.MinLength, this.MaxLength);
            _methodOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
            _fieldOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
            _eventOverloaders = new Dictionary<TypeDefinition, MemberOverloader>();
            _namespaceRenames = new Dictionary<string, string>();
        }

        public bool Perform()
        {
            try
            {
                foreach (TypeDefinition typeDef in AsmDef.Modules.SelectMany(module => module.Types))
                {
                    RenameInType(typeDef);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void RenameInType(TypeDefinition typeDef)
        {
            if (!typeDef.Namespace.StartsWith("Quasar") || typeDef.Namespace.StartsWith("Quasar.Common.Messages") || typeDef.IsEnum)
                return;

            _typeOverloader.GiveName(typeDef);

            typeDef.Namespace = RenameNamespace(typeDef.Namespace);

            MemberOverloader methodOverloader = GetMethodOverloader(typeDef);
            MemberOverloader fieldOverloader = GetFieldOverloader(typeDef);
            MemberOverloader eventOverloader = GetEventOverloader(typeDef);

            if (typeDef.HasNestedTypes)
                foreach (TypeDefinition nestedType in typeDef.NestedTypes)
                    RenameInType(nestedType);

            if (typeDef.HasMethods)
                foreach (MethodDefinition methodDef in
                        typeDef.Methods.Where(methodDef =>
                                !methodDef.IsConstructor && !methodDef.HasCustomAttributes &&
                                !methodDef.IsAbstract && !methodDef.IsVirtual))
                    methodOverloader.GiveName(methodDef);

            if (typeDef.HasFields)
                foreach (FieldDefinition fieldDef in typeDef.Fields)
                    fieldOverloader.GiveName(fieldDef);

            if (typeDef.HasEvents)
                foreach (EventDefinition eventDef in typeDef.Events)
                    eventOverloader.GiveName(eventDef);
        }

        private string RenameNamespace(string originalNamespace)
        {
            if (string.IsNullOrEmpty(originalNamespace))
                return originalNamespace;

            if (!_namespaceRenames.TryGetValue(originalNamespace, out string newNamespace))
            {
                newNamespace = GenerateRandomNamespace();
                _namespaceRenames[originalNamespace] = newNamespace;
            }

            return newNamespace;
        }

        private string GenerateRandomNamespace()
        {
            StringBuilder builder = new StringBuilder();
            int length = _random.Next(MinLength, MaxLength);
            for (int i = 0; i < length; i++)
            {
                builder.Append((char)_random.Next('a', 'z' + 1));
            }
            return builder.ToString();
        }

        private MemberOverloader GetMethodOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._methodOverloaders, typeDef);
        }

        private MemberOverloader GetFieldOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._fieldOverloaders, typeDef);
        }

        private MemberOverloader GetEventOverloader(TypeDefinition typeDef)
        {
            return GetOverloader(this._eventOverloaders, typeDef);
        }

        private MemberOverloader GetOverloader(Dictionary<TypeDefinition, MemberOverloader> overloaderDictionary,
            TypeDefinition targetTypeDef)
        {
            if (!overloaderDictionary.TryGetValue(targetTypeDef, out MemberOverloader overloader))
            {
                overloader = new MemberOverloader(this.MinLength, this.MaxLength);
                overloaderDictionary.Add(targetTypeDef, overloader);
            }
            return overloader;
        }

        private class MemberOverloader
        {
            private bool DoRandom { get; set; }
            private int MinLength { get; set; }
            private int MaxLength { get; set; }
            private readonly Dictionary<string, string> _renamedMembers = new Dictionary<string, string>();
            private readonly char[] _charMap;
            private readonly SafeRandom _random = new SafeRandom();
            private int[] _indices;

            public MemberOverloader(int minLength, int maxLength, bool doRandom = true)
                : this(minLength, maxLength, doRandom, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray())
            {
            }

            private MemberOverloader(int minLength, int maxLength, bool doRandom, char[] chars)
            {
                this._charMap = chars;
                this.DoRandom = doRandom;
                this.MinLength = minLength;
                this.MaxLength = maxLength;
                this._indices = new int[minLength];
            }

            public void GiveName(MemberReference member)
            {
                string currentName = GetCurrentName();
                string originalName = member.ToString();
                member.Name = currentName;
                while (_renamedMembers.ContainsValue(member.ToString()))
                {
                    member.Name = GetCurrentName();
                }
                _renamedMembers.Add(originalName, member.ToString());
            }

            private string GetCurrentName()
            {
                return DoRandom ? GetRandomName() : GetOverloadedName();
            }

            private string GetRandomName()
            {
                StringBuilder builder = new StringBuilder();
                int length = _random.Next(MinLength, MaxLength);

                for (int i = 0; i < length; i++)
                {
                    builder.Append(_charMap[_random.Next(_charMap.Length)]);
                }

                return builder.ToString();
            }

            private string GetOverloadedName()
            {
                IncrementIndices();
                char[] chars = new char[_indices.Length];
                for (int i = 0; i < _indices.Length; i++)
                    chars[i] = _charMap[_indices[i]];
                return new string(chars);
            }

            private void IncrementIndices()
            {
                for (int i = _indices.Length - 1; i >= 0; i--)
                {
                    _indices[i]++;
                    if (_indices[i] >= _charMap.Length)
                    {
                        if (i == 0)
                            Array.Resize(ref _indices, _indices.Length + 1);
                        _indices[i] = 0;
                    }
                    else
                        break;
                }
            }
        }
    }
}