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
			Invoice inv = _invoiceRepository.GetInvoice(payment.Reference);  // change var to Invoice to enable type checking
			string responseMessage = string.Empty; // change var to string 

			if (inv == null) // return error message if no invoice found
			{
				throw new InvalidOperationException("There is no invoice matching this payment");
			}

			// write the conditions as a boolean
			bool checkAmountZero = inv.Amount == 0;
			bool checkInvoiceHasNoPayment = inv.Payments == null || !inv.Payments.Any();
			bool isInvoicePaid = checkAmountZero && checkInvoiceHasNoPayment;

			if (isInvoicePaid)
			{
				if (checkInvoiceHasNoPayment) // if invoice amount is 0 and has no payment, then no payment needed
				{
					responseMessage = "no payment needed";
				}
				else // throw an exception if ivalid invoice (0 amount but have payments, or non-zero amount but no payments)
				{
					throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
				}
			}
			else  // if not paid, proceed to payment process
			{
				// write criteria and variables for payments.
				bool checkInvoiceFullyPaid = inv.Amount == inv.Payments.Sum(x => x.Amount);
				bool checkInvoiceHasPayment = inv.Payments.Sum(x => x.Amount) != 0;
				decimal remainingInvoiceAmount = inv.Amount - inv.AmountPaid;
				bool checkPaymentGreaterThanInvoiceAmount = payment.Amount > remainingInvoiceAmount;
				bool checkPartialPaymentFull = (inv.Amount - inv.AmountPaid) == payment.Amount;
				decimal paymentSubtractInvoiceAmount = payment.Amount - inv.Amount;

				if (!checkInvoiceHasNoPayment) // if invoice already has payments
				{
					if (checkInvoiceHasPayment && checkInvoiceFullyPaid)
					{
						responseMessage = "invoice was already fully paid";
					}
					else if (checkInvoiceHasPayment && checkPaymentGreaterThanInvoiceAmount)
					{
						responseMessage = "the payment is greater than the partial amount remaining";
					}
					else
					{
                        calculateAmountBasedOnInvType(inv, payment); // extract the switch case to a method to improve readability and easier future improvement 

						// at last, set the response message based on full or partial payment
						if (checkPartialPaymentFull)
						{
							responseMessage = "final partial payment received, invoice is now fully paid";
						}
						else
						{
							responseMessage = "another partial payment received, still not fully paid";
						}
					}
				}
				else // otherwise, invoice has no existing payment
				{
					if (paymentSubtractInvoiceAmount > 0)
					{
						responseMessage = "the payment is greater than the invoice amount";
					}
					else
					{
						calculateAmountBasedOnInvType(inv, payment);

						// at last, set the response message based on full or partial payment
						if (paymentSubtractInvoiceAmount == 0)
						{
							responseMessage = "invoice is now fully paid";
						}
						else
						{
							responseMessage = "invoice is now partially paid";
						}
					}
				}
			}

			inv.Save();
			return responseMessage;
		}


		public void calculateAmountBasedOnInvType(Invoice inv, Payment payment)
		{
			switch (inv.Type) // do switch case first to remove further repitition
			{
				case InvoiceType.Standard:
					break; // do nothing (I think here should not inclide tax amount? there should is a bug to include tax to standard invoice in the old code)
				case InvoiceType.Commercial:
					inv.TaxAmount += payment.Amount * 0.14m; // include tax for commercial invoice
					break;
				default:
					throw new ArgumentOutOfRangeException();
            }
            // move below code out of switch statement to avoid repetition
            inv.AmountPaid += payment.Amount;
            inv.Payments.Add(payment);
        }
	}
}