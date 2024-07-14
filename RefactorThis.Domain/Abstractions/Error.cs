using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Abstractions
{
    public class Error
    {
        public string Code { get; }
        public string Name { get; }

        public static readonly Error None = new Error(string.Empty, string.Empty);
        public static readonly Error NullValue = new Error("Error.NullValue", "Null value was provided");

        public Error(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
