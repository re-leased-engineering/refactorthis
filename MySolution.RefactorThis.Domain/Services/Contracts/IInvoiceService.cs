using MySolution.RefactorThis.Domain.Models;

namespace MySolution.RefactorThis.Domain.Services.Contracts
{
    public interface IInvoiceService
    {
        string ProcessPayment(Payment payment);
    }
}
