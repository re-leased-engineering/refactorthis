using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain
{
    public class Invoice
	{
		public decimal Amount { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		public decimal GetTotalAmountPaid()
		{
			if (Payments == null || Payments.Count == 0) return 0M;

			return Payments.Where(x => x.IsProcessed).Sum(x => x.AmountPaid);
		}

		public decimal GetTotalAmount()
		{
			return Amount + TaxAmount;
		}

        public InvoiceType Type { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}