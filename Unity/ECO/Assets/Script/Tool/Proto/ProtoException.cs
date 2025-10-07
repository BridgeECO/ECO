using System;

namespace ECO.Tool.Proto
{
    public class ProtoException<PRT> : Exception where PRT : IProto
    {
        public ProtoException(string message)
        {
            _msg = message;
        }

        public override string Message => $"Prt({typeof(PRT).Name}), Msg({_msg})";
        private string _msg = "";
    }

    public class ProtoException : Exception
    {
        public ProtoException(string message) : base(message)
        {

        }
    }
}
