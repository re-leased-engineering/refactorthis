using RefactorThis.Domain;

namespace RefactorThis.Services;

public record InvoiceDto
{
    public Guid Id { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal Amount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TaxPercentage { get; init; }
    public IEnumerable<PaymentDto> Payments { get; init; } = [];
    public InvoiceType Type { get; set; }
}