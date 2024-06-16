using System;

namespace RefactorThis.Domain
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
		Invoice? GetInvoice(string reference);
		Invoice? GetInvoice(Guid invoiceId);
		void Add(Invoice invoice);
		void Update(Invoice invoice);
    }
    
    public interface IAggregateRoot { }

    public interface IRepository<T> where T : IAggregateRoot
    {
	    void SaveChanges();
    }
}