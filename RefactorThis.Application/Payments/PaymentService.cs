using RefactorThis.Application.Shared;
using RefactorThis.Application.Shared.Payments;
using RefactorThis.Application.Shared.Payments.DTOs;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Repositories.Interfaces;

namespace RefactorThis.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }
    
    public async Task<string> ProcessPaymentAsync(ProcessPaymentDto paymentDto)
    {   
        // could implement mapper
        var payment = new Payment
        {
            Amount = paymentDto.Amount,
            InvoiceId = paymentDto.ReferenceKey
        };
        
        var invoice = await _invoiceRepository.FindByAsync(q => q.Id == payment.InvoiceId);
        if (invoice == null)
        {
            throw new InvalidOperationException("There is no invoice matching this payment");
        }

        if (invoice.Amount == 0)
        {
            return ValidateInvoiceState(invoice.Payments);
        }

        var responseMessage = invoice.Payments != null && invoice.Payments.Any()
            ? HandleExistingPayments(invoice, payment)
            : HandleNoExistingPayments(invoice, payment);

        await _invoiceRepository.Save(invoice);
        await _paymentRepository.Add(payment);
        return responseMessage;
    }

    private string HandleNoExistingPayments(Invoice invoice, Payment payment)
    {
        return payment.Amount > invoice.Amount 
            ? "The payment is greater than the invoice amount" 
            : ProcessPaymentAmount(invoice, payment, invoice.Amount == payment.Amount, true);
    }

    private string HandleExistingPayments(Invoice invoice, Payment payment)
    {
        var totalPayments = invoice.Payments?.Sum(x => x.Amount);
        var remainingAmount = invoice.Amount - invoice.AmountPaid;

        if (totalPayments == invoice.Amount)
        {
            return "Invoice was already fully paid";
        }

        if (payment.Amount > remainingAmount)
        {
            return "The payment is greater than the partial amount remaining";
        }
        
        return ProcessPaymentAmount(invoice, payment, remainingAmount == payment.Amount, false);
    }

    private static string ProcessPaymentAmount(Invoice invoice, Payment payment, bool isFinalPartialPayment, bool isFirstPayment)
    {
        string response;
        
        switch (invoice.Type)
        {
            default:
                throw new ArgumentOutOfRangeException();
            case InvoiceType.Standard:
            {
                if (isFirstPayment)
                {
                    invoice.TaxAmount += payment.Amount * AppConsts.CommercialTaxRate;
                }
                
                break;
            }
            case InvoiceType.Commercial:
            {
                invoice.TaxAmount += payment.Amount * AppConsts.CommercialTaxRate;
                break;
            }
        }
        
        invoice.AmountPaid += payment.Amount;
        invoice.Payments ??= new List<Payment>();
        invoice.Payments.Add(payment);

        if (isFirstPayment)
        {
            response = isFinalPartialPayment
                ? "Invoice is now fully paid"
                : "Invoice is now partially paid";
        }
        else
        {
            response = isFinalPartialPayment
                ? "Final partial payment received, invoice is now fully paid"
                : "Another partial payment received, still not fully paid";
        }
        
        return response;
    }

    private static string ValidateInvoiceState(ICollection<Payment>? payments)
    {
        if (payments == null || !payments.Any())
        {
            return "No payment needed";
        }
        
        throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and no payments");
    }
}