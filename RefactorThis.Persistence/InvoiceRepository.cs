using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence {
	public class InvoiceRepository
	{
		private readonly List<Invoice> _invoices = new List<Invoice>();

		public Invoice GetInvoice(string reference)
		{
			if (string.IsNullOrWhiteSpace(reference))
			{
				return _invoices.FirstOrDefault();
			}

			return _invoices.FirstOrDefault(i =>
				string.Equals(i.Reference, reference, StringComparison.OrdinalIgnoreCase));
		}

		public void UpsertInvoice(Invoice invoice)
		{
			if (invoice == null) throw new ArgumentNullException(nameof(invoice));

			Invoice existing = null;
			if (!string.IsNullOrWhiteSpace(invoice.Reference))
			{
				existing = _invoices.FirstOrDefault(i =>
					string.Equals(i.Reference, invoice.Reference, StringComparison.OrdinalIgnoreCase));
			}

			if (existing == null)
			{
				existing = _invoices.FirstOrDefault(i => ReferenceEquals(i, invoice));
			}

			if (existing != null)
			{
				var index = _invoices.IndexOf(existing);
				_invoices[index] = invoice;
			}
			else
			{
				_invoices.Add(invoice);
			}
		}
	}
}