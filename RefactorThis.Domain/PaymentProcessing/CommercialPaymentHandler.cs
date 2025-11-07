using RefactorThis.Persistence;

namespace RefactorThis.Domain.PaymentProcessing
{
    public class CommercialPaymentHandler : BasePaymentHandler
    {
        public override bool CanHandle(Invoice invoice)
        {
            if (invoice == null) return false;
            return invoice.Type == InvoiceType.Commercial;
        }

        protected override void ApplyAdditionalRules(Invoice invoice, Payment payment)
        {
            invoice.TaxAmount += payment.Amount * 0.14m;
        }
    }
}