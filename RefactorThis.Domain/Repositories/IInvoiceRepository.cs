using RefactorThis.Domain.Entities;

namespace RefactorThis.Domain.Repositories
{
    public interface IInvoiceRepository
    {
        void Add(Invoice invoice);
        Invoice GetInvoice(string reference);
        void SaveInvoice(Invoice invoice);
    }
}