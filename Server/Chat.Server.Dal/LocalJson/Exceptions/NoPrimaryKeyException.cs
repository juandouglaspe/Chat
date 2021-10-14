using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Server.Dal.LocalJson.Exceptions
{

    [Serializable]
    public class NoPrimaryKeyException : Exception
    {
        public NoPrimaryKeyException() : base("The model does not contains PrimaryKey parameter") { }
        public NoPrimaryKeyException(string message) : base(message) { }
        public NoPrimaryKeyException(string message, Exception inner) : base(message, inner) { }
        protected NoPrimaryKeyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
