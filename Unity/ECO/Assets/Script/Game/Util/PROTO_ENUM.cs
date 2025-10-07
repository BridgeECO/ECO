using ECO.Tool.Proto;
using System;

namespace ECO
{
    public static partial class PROTO
    {
        private class EnumReigster : IProtoEnumRegister
        {
            public EnumReigster() { }

            Type IProtoEnumRegister.ConvertStrToType(string str)
            {
                throw new NotImplementedException();
            }
        }
    }
}
