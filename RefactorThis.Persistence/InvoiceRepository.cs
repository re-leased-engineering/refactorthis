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

	public void SaveInvoice(Invoice invoice)
	{
		switch (action)
		{
			case "add":
				if (invoice.Id == Guid.Empty)
				{
					invoice.Id = Guid.NewGuid();
				}
				return;
			case "edit":
				if (invoice.Id == Guid.Empty)
				{
					throw new InvalidOperationException("Cannot be updated.");
				}
				break;
		}
	
		Console.WriteLine("Should save in database..." + JsonSerializer.Serialize(invoice));
		action = "";
	}

	public void Add(Invoice invoice)
	{
		action = "add";
		_invoices.Add(invoice);
	}

	public void Update(Invoice invoice)
	{
		action = "edit";
	}
}