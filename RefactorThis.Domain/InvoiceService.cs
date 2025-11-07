using System;
using System.Linq;
using RefactorThis.Persistence;
using RefactorThis.Domain.PaymentProcessing;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly IPaymentHandler[] _handlers;

        public InvoiceService(InvoiceRepository invoiceRepository, IPaymentHandler[] handlers)
        {
            _invoiceRepository = invoiceRepository;
            _handlers = handlers;
        }

        public string ProcessPayment(Payment payment)
        {
            var inv = _invoiceRepository.GetInvoice(payment.Reference);
            if (inv == null)
                throw new InvalidOperationException("There is no invoice matching this payment");

            var handler = _handlers.FirstOrDefault(h => h.CanHandle(inv));
            if (handler == null)
                throw new InvalidOperationException("No handler found for invoice type");

            return handler.ProcessPayment(inv, payment);
        }

        
    }
}