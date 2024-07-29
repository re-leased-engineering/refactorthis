using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Errors;
using RefactorThis.Domain.Shared;
using System.Net.Http.Headers;

namespace RefactorThis.Domain.Entities
{
    public class Invoice
	{
		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		
		public InvoiceType Type { get; set; }


		public static Result ValidatePayment(Invoice inv, Payment payment)
		{
			if(IsNoPaymentNeeded(inv,payment))
			{
                return Result.Failure(InvoiceErrors.NoPaymentNeeded);
			}

            if (IsInvoiceInvalidState(inv, payment))
            {
                return Result.Failure(InvoiceErrors.InvoiceInvalidState);
            }

            if (IsHasPartialPayment(inv))
			{
				if (IsInvoiceFullyPaid(inv))
				{
                    return Result.Failure(InvoiceErrors.InvoiceAlreadyFullPaid);
                }

				if (IsPaymentGreaterThanPartialAmountRemaining(inv, payment))
				{
                    return Result.Failure(InvoiceErrors.PaymentIsGreaterthanPartialAmountRemaining);
                }
			}

            if (payment.Amount > inv.Amount)
            {
                return Result.Failure(InvoiceErrors.PaymentIsGreaterthanInvoiceAmount);
            }

            return Result.Success();
        }
		public static bool IsHasPartialPayment(Invoice inv) => inv.Payments is not null && inv.Payments.Any();

		private static bool IsInvoiceFullyPaid(Invoice inv) => inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount);
		private static bool IsPaymentGreaterThanPartialAmountRemaining(Invoice inv, Payment payment) => inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid);
		private static bool IsNoPaymentNeeded(Invoice inv, Payment payment) => inv.Amount == 0 && (inv.Payments is null || !inv.Payments.Any());
        private static bool IsInvoiceInvalidState(Invoice inv, Payment payment) => inv.Amount == 0 && inv.Payments is not null && inv.Payments.Any();
    }
}