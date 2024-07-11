using RefactorThis.Domain1.Enums;
using RefactorThis.Domain1.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain1.Models.Entities
{
    public class Invoice
    {
        private readonly IInvoiceRepository _invoiceRepository;
        public Invoice(IInvoiceRepository invoiceRepository)
        {
                _invoiceRepository = invoiceRepository;
        }
        public async Task SaveAsync()
        {
          await _invoiceRepository.SaveInvoiceAsync(this);
        }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment>? Payments { get; set; } = new List<Payment>();
        public InvoiceType Type { get; set; }
    }
}
