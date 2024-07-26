using MySolution.RefactorThis.Domain.Enums;
using MySolution.RefactorThis.Domain.Models;
using MySolution.RefactorThis.Domain.Repositories.Contracts;
using MySolution.RefactorThis.Domain.Services.Contracts;

namespace MySolution.RefactorThis.Domain.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            string? responseMessage;

            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (IsValidInvoice(invoice, payment, out responseMessage))
            {
                ApplyPaymentToInvoice(invoice, payment, out responseMessage);

                _invoiceRepository.SaveInvoice(invoice);
            }

            return responseMessage;
        }
        
        private bool IsValidInvoice(Invoice invoice, Payment payment, out string responseMessage)
        {
            responseMessage = string.Empty;

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            else if (invoice.Amount == 0 && invoice.Payments != null && invoice.Payments.Any())
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }
            else if (invoice.Amount == 0 && (invoice.Payments == null || !invoice.Payments.Any()))
            {
                responseMessage = "no payment needed";
            }
            else
            {
                if (invoice.Payments != null && invoice.Payments.Any())
                {
                    if (invoice.Payments.Sum(x => x.Amount) != 0 && invoice.Payments.Sum(x => x.Amount) == invoice.Amount)
                    {
                        responseMessage = "invoice was already fully paid";
                    }
                    else if (invoice.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
                    {
                        responseMessage = "the payment is greater than the partial amount remaining";
                    }
                }
                else
                {
                    if (payment.Amount > invoice.Amount)
                    {
                        responseMessage = "the payment is greater than the invoice amount";
                    }
                }
            }

            return string.IsNullOrWhiteSpace(responseMessage);
        }

        private void ApplyPaymentToInvoice(Invoice invoice, Payment payment, out string responseMessage)
        {
            if (invoice.Payments != null && invoice.Payments.Any())
            {
                if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
                {
                    responseMessage = "final partial payment received, invoice is now fully paid";
                }
                else
                {
                    responseMessage = "another partial payment received, still not fully paid";
                }

                AddPaymentToInvoiceAndIncrementAmountPaid(payment, invoice);
            }
            else
            {
                invoice.Payments = new List<Payment>();

                if (payment.Amount < invoice.Amount)
                {
                    responseMessage = "invoice is now partially paid";
                }
                else
                {
                    responseMessage = "invoice is now fully paid";
                }

                AddPaymentToInvoiceAndUpdateAmountPaid(payment, invoice);
            }
        }

        private void AddPaymentToInvoiceAndUpdateAmountPaid(Payment payment, Invoice inv)
        {
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                case InvoiceType.Commercial:
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount = payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddPaymentToInvoiceAndIncrementAmountPaid(Payment payment, Invoice inv)
        {
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
                    inv.Payments.Add(payment);
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
