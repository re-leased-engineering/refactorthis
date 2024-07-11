using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain1.Models.Entities
{
    public class Payment
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
