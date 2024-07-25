using MySolution.RefactorThis.Domain.Models;
using MySolution.RefactorThis.Domain.Repositories.Contracts;

namespace MySolution.RefactorThis.Infrastructure.Repositories.Implementations
{
    public class InvoiceRepository: IInvoiceRepository
    {
        private Invoice _invoice;

        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }

        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}
