using RefactorThis.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Interface
{
    public interface IInvoiceService
    {
        // Uses async/await to perform non-blocking I/O operations, ensuring responsiveness and scalability.
        // Accepts a CancellationToken to support cancellation of the operation. Release allocated resources associated with asynchronous operations when they are canceled
        // Why Tasks? Asynchronous Execution, Scalable, Simplifies asynchronous programming with mechanisms, Supports cancellation of asynchronous operations
        Task<string> ProcessPayment(Payment payment, CancellationToken cancellationToken);
        Task SaveInvoice(Invoice invoice, CancellationToken cancellationToken);
        Task<Invoice> GetInvoice(string reference, CancellationToken cancellationToken);
        void CalculateTax(Invoice invoice, Payment payment);
    }
}
