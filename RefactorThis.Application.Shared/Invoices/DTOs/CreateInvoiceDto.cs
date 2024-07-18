using RefactorThis.Application.Shared.Payments.DTOs;

namespace RefactorThis.Application.Shared.Invoices.DTOs;

public class CreateInvoiceDto
{
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public List<PaymentDto>? Payments { get; set; }
}