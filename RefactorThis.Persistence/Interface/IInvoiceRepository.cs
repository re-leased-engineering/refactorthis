using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Interface
{
    public interface IInvoiceRepository
    {
        // Uses async/await to perform non-blocking I/O operations, ensuring responsiveness and scalability.
        // Accepts a CancellationToken to support cancellation of the operation. Release allocated resources associated with asynchronous operations when they are canceled
        // Why Tasks? Asynchronous Execution, Scalable, Simplifies asynchronous programming with mechanisms, Supports cancellation of asynchronous operations
        Task<Invoice> GetInvoice(string reference, CancellationToken cancellationToken);
        Task SaveInvoice(Invoice invoice, CancellationToken cancellationToken);
        Task Add(Invoice invoice);
    }
}
