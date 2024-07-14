using RefactorThis.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Application.ProcessPayment
{
    public sealed class ProcessPaymentCommand : ICommand<string>
    {      
        public decimal Amount { get; private set; }
        public string Reference { get; private set; }   
        public ProcessPaymentCommand(decimal amount, string reference)
        {           
            Amount = amount;
            Reference = reference;         
        }
    }
}
