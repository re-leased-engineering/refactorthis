using System;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private const decimal TaxRate = 0.14m;
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment( Payment payment )
		{
			var invoice = _invoiceRepository.GetInvoice( payment.Reference );

			// Guard clause: Validate invoice exists
			if ( invoice == null )
				throw new InvalidOperationException( "There is no invoice matching this payment" );

			// Guard clause: Handle zero-amount invoices
			if ( invoice.Amount == 0 )
			{
				if ( !invoice.HasPayments )
					return "no payment needed";

				throw new InvalidOperationException( "The invoice is in an invalid state, it has an amount of 0 and it has payments." );
			}

			// Guard clause: Check if invoice is already fully paid
			if ( IsInvoiceFullyPaid( invoice ) )
				return "invoice was already fully paid";

			string responseMessage;

			// Process payment based on whether there are existing payments
			if ( invoice.HasPayments )
			{
				// Handle subsequent payment
				if ( PaymentExceedsRemaining( invoice, payment ) )
				{
					responseMessage = "the payment is greater than the partial amount remaining";
				}
				else
				{
					ApplyPaymentToInvoice( invoice, payment, isFirstPayment: false );

					if ( invoice.Amount == invoice.AmountPaid )
						responseMessage = "final partial payment received, invoice is now fully paid";
					else
						responseMessage = "another partial payment received, still not fully paid";
				}
			}
			else
			{
				// Handle first payment
				if ( payment.Amount > invoice.Amount )
				{
					responseMessage = "the payment is greater than the invoice amount";
				}
				else
				{
					ApplyPaymentToInvoice( invoice, payment, isFirstPayment: true );

					if ( invoice.Amount == payment.Amount )
						responseMessage = "invoice is now fully paid";
					else
						responseMessage = "invoice is now partially paid";
				}
			}

			invoice.Save( );

			return responseMessage;
		}

		private bool IsInvoiceFullyPaid( Invoice invoice )
		{
			if ( !invoice.HasPayments )
				return false;

			var totalPaid = invoice.Payments.Sum( x => x.Amount );
			return totalPaid != 0 && invoice.Amount == totalPaid;
		}

		private bool PaymentExceedsRemaining( Invoice invoice, Payment payment )
		{
			return payment.Amount > invoice.AmountRemaining;
		}

		private void ApplyPaymentToInvoice( Invoice invoice, Payment payment, bool isFirstPayment )
		{
			switch ( invoice.Type )
			{
				case InvoiceType.Standard:
					if ( isFirstPayment )
					{
						invoice.AmountPaid = payment.Amount;
						invoice.TaxAmount = payment.Amount * TaxRate;
					}
					else
					{
						invoice.AmountPaid += payment.Amount;
						// Standard invoices don't accumulate tax on subsequent payments
					}
					invoice.Payments.Add( payment );
					break;

				case InvoiceType.Commercial:
					if ( isFirstPayment )
					{
						invoice.AmountPaid = payment.Amount;
						invoice.TaxAmount = payment.Amount * TaxRate;
					}
					else
					{
						invoice.AmountPaid += payment.Amount;
						invoice.TaxAmount += payment.Amount * TaxRate;
					}
					invoice.Payments.Add( payment );
					break;

				default:
					throw new ArgumentOutOfRangeException( );
			}
		}
	}
}
