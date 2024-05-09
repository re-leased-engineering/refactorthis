using RefactorThis.Persistence.Enums;
using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		//Let's extract this event in this class and move it to service instead.
		//public void Save( )
		//{
		//	_repository.SaveInvoice( this );
		//}

		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		public InvoiceType Type { get; set; }
	}
}