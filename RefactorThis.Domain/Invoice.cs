using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
    public class Invoice : IAggregateRoot
	{
		public Guid Id { get; set; }
		public decimal Amount { get; set; }
		public InvoiceType Type { get; init; }

		public ICollection<Payment> Payments { get; } = new HashSet<Payment>();

		public decimal GetTaxPercentage() => Type == InvoiceType.Commercial ? 0.14M : 0;

		private decimal GetTotalAmountPaid()
		{
			return Payments.Count == 0 ? 0M : Payments.Where(x => x.Status == PaymentStatus.Paid).Sum(x => x.AmountPaid);
		}
	
		public decimal GetTotalAmount() => Amount + GetTaxAmount();
		
        public (bool, string) ProcessPayment(string reference)
        {
	        if (GetTotalAmount() == 0 && !Payments.Any(x => x.Status == PaymentStatus.Paid))
	        {
		        return (false, "no payment needed");
	        }
	        
	        if (IsFullyPaid())
	        {
		        return (false, "invoice was already fully paid");
	        }
	        
	        var payment = Payments.SingleOrDefault(x => x.Reference == reference && x.Status == PaymentStatus.Initialised);

	        if (payment == null)
	        {
		        return (false, "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
	        }
	        
	        if (Payments.Any(x => x.Status == PaymentStatus.Paid) && GetTotalAmountPaid() > 0)
	        {
		        //greater than the total amount
		        if (payment.AmountPaid > GetTotalAmount())
		        {
			        return (false, "the payment is greater than the invoice amount");
		        }
		        //greater than the remaining balance
		        if (payment.AmountPaid > GetUnpaidBalance())
		        {
			        return (false, "the payment is greater than the partial amount remaining") ;
		        }
		        
		        if (payment.AmountPaid == GetUnpaidBalance())
		        {
			        return (true, "invoice is now fully paid");
		        }
	        }
	        else
	        {
		        if (payment.AmountPaid > GetTotalAmount())
		        {
			        return (false, "the payment is greater than the invoice amount");
		        }
		        
		        if (payment.AmountPaid > GetUnpaidBalance())
		        {
			        return (false, "the payment is greater than the partial amount remaining2");
		        }

		        if (payment.AmountPaid == GetUnpaidBalance())
		        {
			        return (true, "final partial payment received, invoice is now fully paid");
		        }

		        if (payment.AmountPaid < GetUnpaidBalance())
		        {
			        return (true, "invoice is now partially paid");
		        } 
	       
		        if (payment.AmountPaid == GetTotalAmount())
		        {
			        return (true, "invoice is now fully paid");
		        }

		       
	        }
			return (false, "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }
        
        private decimal GetUnpaidBalance() => GetTotalAmount() - GetTotalAmountPaid();

        public bool IsFullyPaid()
        {
	        return GetTotalAmountPaid() == GetTotalAmount() &&
	               GetUnpaidBalance() == 0 &&
	               Payments.Any(x => x.Status == PaymentStatus.Paid);
        }
        
        private decimal GetTaxAmount() => Amount * GetTaxPercentage();

	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}