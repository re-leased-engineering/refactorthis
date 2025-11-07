using RefactorThis.Persistence;

namespace RefactorThis.Domain.PaymentProcessing
{
    public interface IPaymentHandler
    {
        bool CanHandle(Invoice invoice);
        string ProcessPayment(Invoice invoice, Payment payment);
    }
}