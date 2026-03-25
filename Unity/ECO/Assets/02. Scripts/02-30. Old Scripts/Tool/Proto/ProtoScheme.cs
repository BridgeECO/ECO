using System;

namespace ECO.Tool.Proto
{
    internal struct ProtoScheme
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public Type SubType { get; private set; }
        public string TypeStr { get; private set; }

        public ProtoScheme(string name, Type type, Type subType)
        {
            this.Name = name;
            this.Type = type;
            this.SubType = subType;
            this.TypeStr = "";

            this.TypeStr = MakeTypeStr(type);
        }

        private string MakeTypeStr(Type type)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(string))
                return "string";
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            if (type == typeof(Array))
                return $"{MakeTypeStr(this.SubType)}[]";
            if (type.IsEnum)
                return type.ToString();

            return "";
        }

        public override string ToString()
        {
            return $"Name({this.Name}), Type({this.Type})";
        }

        public override bool Equals(object obj)
        {
            if (obj is not ProtoScheme scheme)
                return false;

            return this.Name == scheme.Name && this.Type == scheme.Type && this.SubType == scheme.SubType;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
