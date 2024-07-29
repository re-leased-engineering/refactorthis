using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Shared;

namespace RefactorThis.Domain.Services
{
    public interface IInvoiceService
    {
        Result ProcessPayment(Payment payment);
    }
}