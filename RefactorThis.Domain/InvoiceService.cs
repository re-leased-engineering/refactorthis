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

		public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice(payment.Reference);

			var responseMessage = GetResponseMessage(inv, payment);

			return responseMessage;
		}

		public void ComputePayment(Invoice invoice, Payment payment)
		{
            invoice.AmountPaid = payment.Amount;
            invoice.TaxAmount = invoice.Type == InvoiceType.Commercial || (invoice.Type == InvoiceType.Standard && invoice.Payments != null) ? payment.Amount * 0.14m : payment.Amount;
            invoice.Payments.Add(payment);
            invoice.Save();
		}

		public string GetResponseMessage(Invoice invoice, Payment payment)
		{
			string responseMessage = String.Empty;

			if (invoice == null)
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}
			else
			{
				if (invoice.Amount == 0)
				{
					if (invoice.Payments == null || !invoice.Payments.Any())
					{
						return responseMessage = "no payment needed";
					}
					else
					{
						throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
					}
				}
				else
				{
					if (invoice.Payments != null && invoice.Payments.Any())
					{
						if (invoice.Payments.Sum(x => x.Amount) != 0 && invoice.Amount == invoice.Payments.Sum(x => x.Amount))
						{
							return responseMessage = "invoice was already fully paid";
						}
						else if (invoice.Payments.Sum(x => x.Amount) != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
						{
							return responseMessage = "the payment is greater than the partial amount remaining";
						}
						else
						{
							if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
							{
                                ComputePayment(invoice, payment);
                                return responseMessage = "final partial payment received, invoice is now fully paid";
							}
							else
							{
                                ComputePayment(invoice, payment);
                                return responseMessage = "another partial payment received, still not fully paid";
							}
						}
					}
					else
					{
						if (payment.Amount > invoice.Amount)
						{
							return responseMessage = "the payment is greater than the invoice amount";
						}
						else if (invoice.Amount == payment.Amount)
						{
                            ComputePayment(invoice, payment);
                            return responseMessage = "invoice is now fully paid";
						}
						else
						{
                            ComputePayment(invoice, payment);
                            return responseMessage = "invoice is now partially paid";
						}
					}
				}
			}
		}

        }
}