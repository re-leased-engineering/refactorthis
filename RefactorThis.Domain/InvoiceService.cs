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
			var inv = _invoiceRepository.GetInvoice(payment.Reference);
			var responseMessage = string.Empty;

			if (inv == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}

			ValidateInvoice(inv);

			if (inv.Payments != null && inv.Payments.Any())
			{
				HandleExistingPayments(inv, payment, ref responseMessage);
			}
			else
			{
				HandleNewPayment(inv, payment, ref responseMessage);
			}


			inv.Save();
			return responseMessage;
		}

		private void ValidateInvoice(Invoice inv)
		{
			if (inv.Amount == 0)
			{
				if (inv.Payments == null || !inv.Payments.Any())
				{
					throw new InvalidOperationException("No payment needed.");
				}
				else
				{
					throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
				}
			}
		}

		private void HandleExistingPayments(Invoice inv, Payment payment, ref string responseMessage)
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
				HandlePayment(inv, payment);
				if ((inv.Amount - inv.AmountPaid) == 0)
				{
					responseMessage = "Final partial payment received, invoice is now fully paid";
				}
				else
				{
					responseMessage = "Another partial payment received, still not fully paid";
				}
			}
		}

		private void HandleNewPayment(Invoice inv, Payment payment, ref string responseMessage)
		{
			if (payment.Amount > inv.Amount)
			{
				responseMessage = "the payment is greater than the invoice amount";
			}
			else
			{
				HandlePayment(inv, payment);
				if (inv.Amount == inv.AmountPaid)
				{
					responseMessage = "Invoice is now fully paid";
				}
				else
				{
					responseMessage = "Invoice is now partially paid";
				}
			}
		}

		private void HandlePayment(Invoice inv, Payment payment)
		{
			inv.AmountPaid += payment.Amount;
			inv.Payments.Add(payment);
		}


	}
}