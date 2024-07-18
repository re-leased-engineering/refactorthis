using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Repositories.Interfaces;

namespace RefactorThis.Domain.Entities;

public class Invoice : IEntity<int>
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal TaxAmount { get; set; }
    public InvoiceType Type { get; set; }
    public List<Payment>? Payments { get; set; }
}