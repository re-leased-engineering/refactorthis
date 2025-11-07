using System;
using System.Linq;
using System.Collections.Generic;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.PaymentProcessing
{
    public abstract class BasePaymentHandler : IPaymentHandler
    {
        public abstract bool CanHandle(Invoice invoice);

        public virtual string ProcessPayment(Invoice invoice, Payment payment)
        {
            if (invoice == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            if (invoice.Amount == 0)
            {
                if (invoice.Payments == null || !invoice.Payments.Any())
                    return "no payment needed";

                throw new InvalidOperationException(
                    "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            var totalPaid = invoice.Payments?.Sum(x => x.Amount) ?? 0;
            var remainingAmount = invoice.Amount - totalPaid;

            if (totalPaid != 0 && remainingAmount == 0)
                return "invoice was already fully paid";

            if (totalPaid != 0 && payment.Amount > remainingAmount)
                return "the payment is greater than the partial amount remaining";

            if (totalPaid == 0 && payment.Amount > invoice.Amount)
                return "the payment is greater than the invoice amount";

            bool isFinalPayment = payment.Amount == remainingAmount;

            invoice.AmountPaid += payment.Amount;
            if (invoice.Payments == null)
            {
               invoice.Payments = new List<Payment>(); 
            }
    
            invoice.Payments.Add(payment);

            ApplyAdditionalRules(invoice, payment);
            invoice.Save();

            if (isFinalPayment)
                return totalPaid == 0
                    ? "invoice is now fully paid"
                    : "final partial payment received, invoice is now fully paid";

            return totalPaid == 0
                ? "invoice is now partially paid"
                : "another partial payment received, still not fully paid";
        }

        protected virtual void ApplyAdditionalRules(Invoice invoice, Payment payment)
        {
            // Hook for subclasses
        }
    }
}