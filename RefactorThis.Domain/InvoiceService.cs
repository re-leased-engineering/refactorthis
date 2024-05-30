using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

            if (inv == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (inv.Amount == 0)
            {
                return HandleZeroAmountInvoice(inv);
            }

            if (inv.Payments != null && inv.Payments.Any())
            {
                return HandleExistingPayments(inv, payment);
            }

            return HandleNoExistingPayments(inv, payment);
        }

        // Handle invoices with zero amount
        private string HandleZeroAmountInvoice(Invoice inv)
        {
            if (inv.Payments == null || !inv.Payments.Any())
            {
                return "no payment needed";
            }

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        // Handle invoices with existing payments
        private string HandleExistingPayments(Invoice inv, Payment payment)
        {
            var totalPaid = inv.Payments.Sum(x => x.Amount);

            if (totalPaid != 0 && inv.Amount == totalPaid)
            {
                return "invoice was already fully paid";
            }

            if (totalPaid != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            {
                return "the payment is greater than the partial amount remaining";
            }

            return ProcessPaymentAmount(inv, payment);
        }

        // Handle invoices with no existing payments
        private string HandleNoExistingPayments(Invoice inv, Payment payment)
        {
            if (payment.Amount > inv.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            return ProcessPaymentAmount(inv, payment);
        }

        // Process the payment amount and update the invoice
        private string ProcessPaymentAmount(Invoice inv, Payment payment)
        {
            bool isFinalPayment = (inv.Amount - inv.AmountPaid) == payment.Amount;

            switch (inv.Type)
            {
                case InvoiceType.Standard:
                    inv.AmountPaid += payment.Amount;
                    inv.Payments.Add(payment);
                    inv.Save();
                    return isFinalPayment ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid";

                case InvoiceType.Commercial:
                    inv.AmountPaid += payment.Amount;
                    inv.TaxAmount += payment.Amount * 0.14m;
                    inv.Payments.Add(payment);
                    inv.Save();
                    return isFinalPayment ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}