using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RefactorThis.Domain.Models.Entities;

namespace RefactorThis.Domain.Validators
{
    public static class PaymentValidators
    {
        public static bool IsNoPaymentNeeded(Invoice inv)
           => inv.Amount == 0
                && (inv.Payments == null || !inv.Payments.Any());
        public static bool IsPaymentGreaterThanInvoiceAmount(Payment payment, Invoice inv)
           => payment.Amount > inv.Amount;
        public static bool IsInvoiceFullyPaid(Invoice inv)
           => inv.Payments?.Sum(x => x.Amount) != 0
                && inv.Amount == inv.Payments?.Sum(x => x.Amount);
        public static bool IsPaymentGreaterThanRemainingAmount(Payment payment, Invoice inv)
           => inv.Payments?.Sum(x => x.Amount) != 0
                && payment.Amount > inv.Amount - inv.Payments?.Sum(x => x.Amount);

        public static string CheckPaymentValidity(Invoice inv, Payment payment)
        {
            if (IsNoPaymentNeeded(inv))
            {
                return "no payment needed";
            }
            else if (IsPaymentGreaterThanInvoiceAmount(payment, inv))
            {
                return "the payment is greater than the invoice amount";
            }
            else if (IsInvoiceFullyPaid(inv))
            {
                return "invoice was already fully paid";
            }
            else if (IsPaymentGreaterThanRemainingAmount(payment, inv))
            {
                return "the payment is greater than the partial amount remaining";
            }
            return string.Empty;
        }
        public static string CheckIfFinalPartialPayment(Invoice inv, Payment payment)
        {
            if (inv.Amount - inv.AmountPaid == payment.Amount)
            {
                return "final partial payment received, invoice is now fully paid";
            }
            return "another partial payment received, still not fully paid";
        }
        public static string CheckIfInvoiceIsFullyPaid(Invoice inv, Payment payment)
        {
            if (inv.Amount == payment.Amount)
            {
                return "invoice is now fully paid";
            }
            return "invoice is now partially paid";
        }
    }
}
