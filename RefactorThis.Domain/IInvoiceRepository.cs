using System;

namespace RefactorThis.Domain
{
    public interface IInvoiceRepository
    {
		Invoice? GetInvoice(string reference);
		Invoice? GetInvoice(Guid invoiceId);

		void SaveInvoice(Invoice invoice);
		

		void Add(Invoice invoice);
		void Update(Invoice invoice);
    }
}