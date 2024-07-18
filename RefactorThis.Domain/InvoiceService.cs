using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            // _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            string responseMessage = HandlePayment(payment, invoice);

            invoice.Save();

            return responseMessage;
        }

        private string HandlePayment(Payment payment, Invoice invoice)
        {
            string responseMessage = string.Empty;

            if (invoice.Amount == 0)
            {
                ValidateZeroAmountInvoice(invoice, ref responseMessage);
            }
            else
            {
                if (invoice.Payments != null && invoice.Payments.Any())
                {
                    HandlePartialPayments(payment, invoice, ref responseMessage);
                }
                else
                {
                    HandleFullPayment(payment, invoice, ref responseMessage);
                }
            }

            return responseMessage;
        }

        private void ValidateZeroAmountInvoice(Invoice invoice, ref string responseMessage)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                responseMessage = "no payment needed";
            }
            else
            {
                throw new InvalidOperationException("invalid invoice state: amount is 0 but there are payments");
            }
        }

        private void HandlePartialPayments(Payment payment, Invoice invoice, ref string responseMessage)
        {
            decimal totalPaid = invoice.Payments.Sum(x => x.Amount);
            decimal remainingAmount = invoice.Amount - invoice.AmountPaid;

            if (totalPaid != 0 && totalPaid == invoice.Amount)
            {
                responseMessage = "invoice was already fully paid";
            }
            else if (totalPaid != 0 && payment.Amount > remainingAmount)
            {
                responseMessage = "the payment is greater than the partial amount remaining";
            }
            else
            {
                HandleRemainingPartialPayments(payment, invoice, ref responseMessage);
            }
        }

        private void HandleRemainingPartialPayments(Payment payment, Invoice invoice, ref string responseMessage)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);

            if (invoice.Amount - invoice.AmountPaid == 0)
            {
                HandleFinalPartialPayment(invoice, payment);
                responseMessage = "final partial payment received, invoice is now fully paid";
            }
            else
            {
                responseMessage = "another partial payment received, still not fully paid";
            }
        }

        private void HandleFinalPartialPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += invoice.Amount - invoice.AmountPaid;

            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    break;
                case InvoiceType.Commercial:
                    invoice.TaxAmount += CalculateTax(invoice.Amount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            invoice.Payments.Add(payment);
        }

        private void HandleFullPayment(Payment payment, Invoice invoice, ref string responseMessage)
        {
            if (payment.Amount > invoice.Amount)
            {
                responseMessage = "the payment is greater than the invoice amount";
            }
            else
            {
                invoice.AmountPaid = payment.Amount;
                invoice.Payments.Add(payment);

                switch (invoice.Type)
                {
                    case InvoiceType.Standard:
                        invoice.TaxAmount = CalculateTax(payment.Amount);
                        break;
                    case InvoiceType.Commercial:
                        invoice.TaxAmount = CalculateTax(payment.Amount);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                responseMessage = "invoice is now " + (invoice.AmountPaid == invoice.Amount ? "fully" : "partially") + " paid";
            }

            _invoiceRepository.SaveInvoice(invoice);

        }

        private decimal CalculateTax(decimal amount)
        {
            return amount * TaxRate;
        }
    }
}

