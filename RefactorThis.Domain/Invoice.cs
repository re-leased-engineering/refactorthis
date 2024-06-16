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

		public decimal GetUnpaidBalance() => GetTotalAmount() - GetTotalAmountPaid();

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
	        
			if (GetUnpaidBalance() == 0)
			{
				return (false, "invoice was already fully paid");
			}

			if (payment!.AmountPaid == GetTotalAmount())
			{
				return (true, "invoice is now fully paid");
			}

			if (payment.AmountPaid > GetTotalAmount())
			{
				return (false, "the payment is greater than the invoice amount");
			}
			
			if (payment.AmountPaid > GetUnpaidBalance())
			{
				return (false, "the payment is greater than the partial amount remaining") ;
			}

			if (payment.AmountPaid == GetUnpaidBalance())
			{
				return (true, "final partial payment received, invoice is now fully paid");
			}

			if (payment.AmountPaid < GetUnpaidBalance())
			{
				return (true, "invoice is now partially paid");
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