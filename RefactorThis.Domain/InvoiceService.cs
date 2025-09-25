using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using RefactorThis.Persistence.Model;

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
            var inv = _invoiceRepository.GetInvoice(payment.Reference);

			var responseMessage = string.Empty;

			//Sanity Check -Exit early if no invoice
			if (inv == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}
            //Sanity Check -Exit early if payments do not align with amount due
            if (inv.Amount == 0 && inv.Payments != null)
            {
                throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            ValidatePaymentState(inv, payment, ref responseMessage);

            inv.Save();

            return responseMessage;

		}


        private void ValidatePaymentState(Invoice inv, Payment payment, ref string responseMessage)
        {

            if (inv.Amount == 0)
            {
                responseMessage = "no payment needed";
                return;
            }

            //If payments list exists and entries are present
            if (inv.Payments.Any())
            {
                ValidateInvoiceWithPartialPayments(inv, payment, ref responseMessage);
            }
            else
            {
                UpdateInvoiceFirstPayment(inv, payment, ref responseMessage);
            }
        }


        private void ValidateInvoiceWithPartialPayments(Invoice inv, Payment payment, ref string responseMessage)
        {
            if (inv.Payments.Sum(x => x.Amount) != 0 && inv.Amount == inv.Payments.Sum(x => x.Amount))
            {
                responseMessage = "invoice was already fully paid";
            }
            else if (inv.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (inv.Amount - inv.AmountPaid))
            {
                responseMessage = "the payment is greater than the partial amount remaining";
            }
            else
            {
                UpdateInvoiceWithPartialPayment(inv, payment, ref responseMessage);
            }
        }


        private void UpdateInvoiceWithPartialPayment(Invoice inv, Payment payment, ref string responseMessage)
        {
            decimal tax = payment.Amount * 0.14m;

            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);

            if (inv.Type == InvoiceType.Commercial)
            {
                inv.TaxAmount += tax;
            }

            responseMessage = ((inv.Amount - inv.AmountPaid) == payment.Amount) ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid";
        }


        private void UpdateInvoiceFirstPayment(Invoice inv, Payment payment, ref string responseMessage)
        {
            //Sanity check, return if paid amount greater than invoiced amount
            if (payment.Amount > inv.Amount)
            {
                responseMessage = "the payment is greater than the invoice amount";
                return;
            }
            //TODO: Follow up on tax being added only to first payment on Standard invoices
            inv.AmountPaid = payment.Amount;
            inv.TaxAmount = payment.Amount * 0.14m;
            inv.Payments.Add(payment);

            responseMessage = inv.Amount == payment.Amount ? "invoice is now fully paid" : "invoice is now partially paid";
        }

    }
}