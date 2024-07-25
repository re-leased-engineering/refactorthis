using MySolution.RefactorThis.Domain.Enums;

namespace MySolution.RefactorThis.Domain.Models
{
    public class Invoice
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; } = default!;
        public InvoiceType Type { get; set; }

    }
}
