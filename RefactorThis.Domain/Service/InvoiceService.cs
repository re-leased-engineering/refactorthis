using RefactorThis.Persistence;
using RefactorThis.Persistence.IRepository;
using RefactorThis.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Service
{
    public class InvoiceService
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
                responseMessage = "There is no invoice matching this payment";
            }
            else
            {
                ValidateAndProcessInvoice(inv, payment, ref responseMessage);
            }

            _invoiceRepository.SaveInvoice(inv);

            return responseMessage;
        }

        private void ValidateAndProcessInvoice(Invoice inv, Payment payment, ref string responseMessage)
        {
            if (inv.Amount == 0)
            {
                responseMessage = (inv.Payments == null || !inv.Payments.Any()) ? "no payment needed" : "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
            }
            else
            {
                ProcessInvoicePayment(inv, payment, ref responseMessage);
            }
        }

        private void ProcessInvoicePayment(Invoice inv, Payment payment, ref string responseMessage)
        {
            if (inv.Payments != null && inv.Payments.Any())
            {
                HandleInvoiceWithBalancePayments(inv, payment, ref responseMessage);

            }
            else
            {
                HandleNonPaidInvoice(inv, payment, ref responseMessage);
            }
        }

        private void HandleInvoiceWithBalancePayments(Invoice inv, Payment payment, ref string responseMessage)
        {
            decimal totalPayments = inv.Payments.Sum(x => x.Amount);
            if (totalPayments != 0 && inv.Amount == totalPayments)
            {
                responseMessage = "invoice was already fully paid";
            }
            else if (totalPayments != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            {
                responseMessage = "the payment is greater than the partial amount remaining";
            }
            else
            {
                HandleInvoiceWithRemainingBalance(inv, payment, ref responseMessage);
            }
        }

        private void HandleInvoiceWithRemainingBalance(Invoice inv, Payment payment, ref string responseMessage)
        {
            if ((inv.Amount - inv.AmountPaid) == payment.Amount)
            {
                ProcessPaymentForPartiallyPaidInvoice(inv, payment, ref responseMessage, "final partial payment received, invoice is now fully paid");
            }
            else
            {
                ProcessPaymentForPartiallyPaidInvoice(inv, payment, ref responseMessage, "another partial payment received, still not fully paid");
            }
        }

        private void HandleNonPaidInvoice(Invoice inv, Payment payment, ref string responseMessage)
        {
            if (payment.Amount > inv.Amount)
            {
                responseMessage = "the payment is greater than the invoice amount";
            }
            else if (inv.Amount == payment.Amount)
            {
                ProcessPaymentForNonPaidInvoice(inv, payment, ref responseMessage, "invoice is now fully paid");
            }
            else
            {
                ProcessPaymentForNonPaidInvoice(inv, payment, ref responseMessage, "invoice is now partially paid");
            }
        }

        private void ProcessPaymentForPartiallyPaidInvoice(Invoice inv, Payment payment, ref string responseMessage, string responseMessageContent)
        {
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
                    inv.Payments.Add(payment);
                    responseMessage = responseMessageContent;
                    break;
                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    responseMessage = responseMessageContent;
                    break;
                default:
                    responseMessage = "invoice type not recognized";
                    break;
            }
        }

        private void ProcessPaymentForNonPaidInvoice(Invoice inv, Payment payment, ref string responseMessage, string responseMessageContent)
        {
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                case InvoiceType.Commercial:
                    inv.AmountPaid = payment.Amount;
                    inv.TaxAmount = payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    responseMessage = responseMessageContent;
                    break;
                default:
                    responseMessage = "invoice type not recognized";
                    break;
            }
        }

    }
}
