using System.Collections.Generic;

namespace RefactorThis.Persistence
{
	public class Invoice
	{
		private readonly InvoiceRepository _repository;
		public Invoice( InvoiceRepository repository )
		{
			_repository = repository;
			// Ensure Payments is initialized to safely add Payments
			Payments = new List<Payment>();
			// Default to Standard to provide a sensible default
			Type = InvoiceType.Standard;
		}

		public void Save( )
		{
			_repository.UpsertInvoice( this );
		}

		public decimal Amount { get; set; }
		public decimal AmountPaid { get; set; }
		public decimal TaxAmount { get; set; }
		public List<Payment> Payments { get; set; }
		public InvoiceType Type { get; set; }
		public string Reference { get; set; }
	}

	public enum InvoiceType
	{
		Standard,
		Commercial
	}
}