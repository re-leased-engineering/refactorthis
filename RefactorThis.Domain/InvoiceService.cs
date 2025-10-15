using System;
using System.Linq;
using System.Collections.Generic;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		private const decimal TAX_RATE = 0.14m;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.Reference );

			// Fail fast if invoice not found
			if (inv == null)
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			// Ensure payments collection is present so we can safely Add to it later.
			inv.Payments = inv.Payments ?? new List<Payment>();

			string responseMessage;

			if ( inv.Amount == 0 )
			{
				if ( !inv.Payments.Any() )
				{
					responseMessage = "no payment needed";
				}
				else
				{
					throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
				}
			}
			else
			{
				if ( inv.Payments.Any() )
				{
					var paymentsSum = inv.Payments.Sum( x => x.Amount );
					if ( paymentsSum != 0 && inv.Amount == paymentsSum )
					{
						responseMessage = "invoice was already fully paid";
					}
					else if ( paymentsSum != 0 && payment.Amount > ( inv.Amount - inv.AmountPaid ) )
					{
						responseMessage = "the payment is greater than the partial amount remaining";
					}
					else
					{
						// There are previous payments and this payment is valid to apply
						bool isFinalPayment = ( inv.Amount - inv.AmountPaid ) == payment.Amount;
						ApplyPayment(inv, payment, replaceAmountPaid: false, firstPayment: false);
						responseMessage = isFinalPayment
							? "final partial payment received, invoice is now fully paid"
							: "another partial payment received, still not fully paid";
					}
				}
				else
				{
					if ( payment.Amount > inv.Amount )
					{
						responseMessage = "the payment is greater than the invoice amount";
					}
					else if ( inv.Amount == payment.Amount )
					{
						// First payment that exactly equals the invoice amount
						ApplyPayment(inv, payment, replaceAmountPaid: true, firstPayment: true);
						responseMessage = "invoice is now fully paid";
					}
					else
					{
						// First payment and it's partial
						ApplyPayment(inv, payment, replaceAmountPaid: true, firstPayment: true);
						responseMessage = "invoice is now partially paid";
					}
				}
			}
			
			inv.Save();

			return responseMessage;
		}

		private void ApplyPayment(Invoice inv, Payment payment, bool replaceAmountPaid, bool firstPayment)
		{
			if (replaceAmountPaid)
			{
				inv.AmountPaid = payment.Amount;
			}
			else
			{
				inv.AmountPaid += payment.Amount;
			}

			// Tax behavior mirrors original code: on the very first payment set TaxAmount = amount * rate;
			// on subsequent payments only Commercial invoices accumulate additional tax.
			if (firstPayment)
			{
				inv.TaxAmount = payment.Amount * TAX_RATE;
			}
			else
			{
				if (inv.Type == InvoiceType.Commercial)
				{
					inv.TaxAmount += payment.Amount * TAX_RATE;
				}
			}

			inv.Payments.Add(payment);
		}
	}
}