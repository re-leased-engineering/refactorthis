using System;
using System.Linq;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using RefactorThis.Persistence;

namespace RefactorThis.Core.Services
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
            var inv = _invoiceRepository.Get(payment.Reference);
            if (inv == null) throw new InvalidOperationException("There is no invoice matching this payment");

            if (inv.Amount == 0) return HandleZeroAmountInvoice(inv);

            var responseMessage = HasPayments(inv)
                ? HandleExistingPayments(inv, payment)
                : HandleNoExistingPayments(inv, payment);

            _invoiceRepository.Save(inv);

            return responseMessage;
        }

        private string HandleZeroAmountInvoice(Invoice inv)
        {
            if (inv.Payments == null || !inv.Payments.Any())
                return "no payment needed";
            throw new InvalidOperationException(
                "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private bool HasPayments(Invoice inv) => inv.Payments != null && inv.Payments.Any();

        private string HandleExistingPayments(Invoice inv, Payment payment)
        {
            var totalPaid = inv.Payments.Sum(x => x.Amount);

            if (totalPaid != 0)
            {
                if (inv.Amount == totalPaid) return "invoice was already fully paid";

                if (payment.Amount > inv.Amount - inv.AmountPaid)
                    return "the payment is greater than the partial amount remaining";
            }

            return ProcessPartialPayment(inv, payment);
        }

        private string HandleNoExistingPayments(Invoice inv, Payment payment)
        {
            if (payment.Amount > inv.Amount) return "the payment is greater than the invoice amount";

            return ProcessFullOrPartialPayment(inv, payment);
        }

        private string ProcessPartialPayment(Invoice inv, Payment payment)
        {
            if (inv.Amount - inv.AmountPaid == payment.Amount) return ProcessFinalPartialPayment(inv, payment);

            return ProcessAnotherPartialPayment(inv, payment);
        }

        private string ProcessFullOrPartialPayment(Invoice inv, Payment payment)
        {
            var isFullPayment = inv.Amount == payment.Amount;

            UpdateInvoicePayment(inv, payment);

            return isFullPayment ? "invoice is now fully paid" : "invoice is now partially paid";
        }

        private string ProcessFinalPartialPayment(Invoice inv, Payment payment)
        {
            UpdateInvoicePayment(inv, payment);

            return "final partial payment received, invoice is now fully paid";
        }

        private string ProcessAnotherPartialPayment(Invoice inv, Payment payment)
        {
            UpdateInvoicePayment(inv, payment);

            return "another partial payment received, still not fully paid";
        }

        private void UpdateInvoicePayment(Invoice inv, Payment payment)
        {
            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);

            if (inv.Type == InvoiceType.Commercial) inv.TaxAmount += payment.Amount * 0.14m;
        }
    }
}