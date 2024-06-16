using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
    public class Invoice
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public decimal TaxAmount { get; set; }
		public ICollection<Payment> Payments { get; } = new HashSet<Payment>();
		public decimal GetTotalAmountPaid()
		{
			return Payments.Count == 0 ? 0M : Payments.Where(x => x.Status == PaymentStatus.Paid).Sum(x => x.AmountPaid);
		}

		public decimal GetTotalBalance() => GetTotalAmount() - GetTotalAmountPaid();

		public decimal GetTotalAmount()
		{
			return Amount + TaxAmount;
		}

        public InvoiceType Type { get; set; }

        public (bool, string) ProcessPayment(string reference)
        {
	        if (GetTotalAmount() == 0 || Payments.Count == 0)
	        {
		        return (false, "no payment needed");
	        }
	        
	        var payment = Payments.SingleOrDefault(x => x.Reference == reference && x.Status == PaymentStatus.Initialised);

			if (GetTotalBalance() == 0)
			{
				return (false, "invoice was already fully paid");
			}

			if (payment!.AmountPaid > GetTotalBalance())
			{
				return (false, "the payment is greater than the partial amount remaining");
			}

			if (payment.AmountPaid == GetTotalBalance())
			{
				//if final
				return (true, "final partial payment received, invoice is now fully paid");
			}
			
			if (payment.AmountPaid <= GetTotalBalance() && payment.AmountPaid != 0)
			{
				//partial 
				return (true, "another partial payment received, still not fully paid");
			}
			
			return (false, "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}