using System;

namespace ECO.Tool.Proto
{
    public interface IProto
    {

    }

    public interface IProtoEnumRegister
    {
        public Type ConvertStrToType(string str);
    }
}
