using System.Collections.Generic;
using RefactorThis.Domain.Common;
using RefactorThis.Domain.Enums;

namespace RefactorThis.Domain.Entities
{
    public class Invoice : Entity
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }

        public InvoiceType Type { get; set; }
    }
}