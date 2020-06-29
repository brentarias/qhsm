using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace qf4net
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MessageAttribute : Attribute
    {
        public Type Type;

        public MessageAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
