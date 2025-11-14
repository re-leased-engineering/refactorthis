using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            var responseMessage = string.Empty;

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            else
            {
                if (invoice.Amount == 0)
                {
                    if (!invoice.InvoiceHasPayments)
                    {
                        responseMessage = "no payment needed";
                    }
                    else
                    {
                        throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                    }
                }
                else
                {
                    if (invoice.InvoiceHasPayments)
                    {
                        if (invoice.InvoiceIsAlreadyFullyPaid)
                        {
                            responseMessage = "invoice was already fully paid";
                        }
                        else if (invoice.IsPaymentsGreaterThanThePartial(payment))
                        {
                            responseMessage = "the payment is greater than the partial amount remaining";
                        }
                        else
                        {
                            responseMessage = invoice.PartialPay(payment);

                        }
                    }
                    else
                    {
                        if (payment.Amount > invoice.Amount)
                        {
                            responseMessage = "the payment is greater than the invoice amount";
                        }
                        else
                        {
                            responseMessage = invoice.FullPay(payment);

                        }
                    }
                }
            }

            invoice.Save();

            return responseMessage;
        }
    }
}