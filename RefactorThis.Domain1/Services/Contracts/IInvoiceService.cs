using RefactorThis.Domain1.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain1.Services.Contracts
{
    public interface IInvoiceService
    {
        public Task<string> ProcessPaymentAsync(Payment payment);
    }
}
