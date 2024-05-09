using RefactorThis.Persistence.Interface;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Persistence
{
    //Register this in Dependency Injection Service
	public class InvoiceRepository : IInvoiceRepository
	{
		private Invoice _invoice;

        public Task<Invoice> GetInvoice(string reference, CancellationToken cancellationToken = default)
        {
            // Simulate retrieving the invoice asynchronously from a database
            // For example:
            // return _databaseContext.Invoices.FindAsync(reference, cancellationToken);
            return Task.FromResult(_invoice);
		}

        public Task SaveInvoice(Invoice invoice, CancellationToken cancellationToken = default)
        {
            // Simulate saving the invoice to the database asynchronously
            // For example:
            // return _databaseContext.Invoices.AddAsync(invoice, cancellationToken);
            return Task.CompletedTask;
		}

        public Task Add(Invoice invoice)
        {
            // Simulate adding the invoice to the repository
            // For example:
            // _databaseContext.Invoices.Add(invoice);
            _invoice = invoice;
            return Task.CompletedTask;
        }
    }
}