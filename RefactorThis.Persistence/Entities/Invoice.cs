using System.Collections.Generic;
using RefactorThis.Persistence.Contracts;

namespace RefactorThis.Persistence.Entities
{
	public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public string Reference { get; set; }
        public List<Payment> Payments { get; set; }
        public InvoiceType Type { get; set; }

        private readonly IInvoiceRepository _repository;

        public Invoice(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        public void Save()
        {
            _repository.SaveInvoice(this);
        }
    }

    public enum InvoiceType
    {
        Standard,
        Commercial
    }
}