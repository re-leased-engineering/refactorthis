using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RefactorThis.Domain;

namespace RefactorThis.Persistence;

public class InvoiceRepository : IInvoiceRepository
{
	private readonly ICollection<Invoice> _invoices = new HashSet<Invoice>();
	private readonly ICollection<Invoice> _temporaryInvoices = new List<Invoice>();
	private string action = "add";

	public Invoice? GetInvoice(string reference)
	{
		var invoice = _invoices
			.FirstOrDefault(x => x.Payments.Select(p => p.Reference).Contains(reference));
		return invoice;
	}

	public Invoice? GetInvoice(Guid invoiceId)
	{
		return _invoices.SingleOrDefault(x => x.Id == invoiceId);
	}

	public void SaveChanges()
	{
		switch (action)
		{
			case "add":
				foreach (var invoice in _temporaryInvoices)
				{
					if (invoice.Id == Guid.Empty)
					{
						invoice.Id = Guid.NewGuid();
					}

					_invoices.Add(invoice);
					Console.WriteLine("Should save in database..." + JsonSerializer.Serialize(invoice));
				}
				return;
			case "edit":
				foreach (var invoice in _temporaryInvoices)
				{
					var savedInvoice = _invoices.SingleOrDefault(x => x.Id == invoice.Id);

					if (savedInvoice == null)
					{
						throw new InvalidOperationException("No matching tracked entity");
					}

					_invoices.Remove(savedInvoice);
					_invoices.Add(invoice);
					Console.WriteLine("Should save in database..." + JsonSerializer.Serialize(invoice));
				}
				break;
		}
	
		_temporaryInvoices.Clear();
		action = "";
	}

	public void Add(Invoice invoice)
	{
		action = "add";
		_temporaryInvoices.Add(invoice);
	}

	public void Update(Invoice invoice)
	{
		action = "edit";
		_temporaryInvoices.Add(invoice);
	}

}