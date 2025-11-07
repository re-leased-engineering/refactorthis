using RefactorThis.Persistence;

namespace RefactorThis.Domain.PaymentProcessing
{
    public class StandardPaymentHandler : BasePaymentHandler
    {
        public override bool CanHandle(Invoice invoice)
        {
            if (invoice == null) return false;
            return invoice.Type == InvoiceType.Standard;
        }
    }
            
}