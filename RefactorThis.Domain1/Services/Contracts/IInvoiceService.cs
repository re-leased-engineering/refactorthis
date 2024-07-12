using RefactorThis.Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Services.Contracts
{
    public interface IInvoiceService
    {
        public Task<string> ProcessPaymentAsync(Payment payment);
    }
}
