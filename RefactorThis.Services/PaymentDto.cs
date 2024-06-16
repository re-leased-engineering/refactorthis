using RefactorThis.Domain;

namespace RefactorThis.Services;

public record PaymentDto
{
    public decimal AmountPaid { get; init; }
    public string? Reference { get; init; }
    public PaymentStatus Status { get; init; } = PaymentStatus.New;
    public string Remarks { get; init; }
}