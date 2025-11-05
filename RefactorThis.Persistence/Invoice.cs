using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		private readonly InvoiceRepository _repository;
		public Invoice( InvoiceRepository repository )
		{
			_repository = repository;
		}

		public void Save( )
		{
			_repository.SaveInvoice( this );
		}

		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }

		public InvoiceType Type { get; set; }

		public bool HasPayments
		{
			get { return Payments != null && Payments.Any( ); }
		}

		public decimal AmountRemaining
		{
			get { return Amount - AmountPaid; }
		}
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}
