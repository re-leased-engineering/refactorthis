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
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            var responseMessage = string.Empty;

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            else if (inv.Amount == 0 && inv.Payments != null && inv.Payments.Any())
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }
            else if (inv.Amount == 0 && (inv.Payments == null || !inv.Payments.Any()))
            {
                responseMessage = "no payment needed";
            }
            else 
            {
                if (inv.Payments != null && inv.Payments.Any())
                {
                    if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Payments.Sum(x => x.Amount) == inv.Amount)
                    {
                        responseMessage = "invoice was already fully paid";
                    }
                    else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
                    {
                        responseMessage = "the payment is greater than the partial amount remaining";
                    }
                    else
                    {
                        if ((inv.Amount - inv.AmountPaid) == payment.Amount)
                        {
                            responseMessage = "final partial payment received, invoice is now fully paid";
                        }
                        else
                        {
                            responseMessage = "another partial payment received, still not fully paid";
                        }

                        AddPaymentToInvoiceAndIncrementAmountPaid(payment, inv);
                    }
                }
                else
                {
                    inv.Payments = new List<Payment>();

                    if (payment.Amount > inv.Amount)
                    {
                        responseMessage = "the payment is greater than the invoice amount";
                    }
                    else
                    {
                        if (payment.Amount < inv.Amount)
                        {
                            responseMessage = "invoice is now partially paid";
                        }
                        else
                        {
                            responseMessage = "invoice is now fully paid";
                        }

                        AddPaymentToInvoiceAndUpdateAmountPaid(payment, inv);
                    }
                }
            }


            _invoiceRepository.SaveInvoice(inv);

            return responseMessage;
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
