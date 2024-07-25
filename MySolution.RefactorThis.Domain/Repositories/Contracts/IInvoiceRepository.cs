using MySolution.RefactorThis.Domain.Models;

namespace MySolution.RefactorThis.Domain.Repositories.Contracts
{
    public interface IInvoiceRepository
    {
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
        void Add(Invoice invoice);
    }
}
