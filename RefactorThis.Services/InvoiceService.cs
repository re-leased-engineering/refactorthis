using RefactorThis.Domain;

namespace RefactorThis.Services
{
	public class InvoiceService(IInvoiceRepository invoiceRepository)
	{
		/// <summary>
		/// Returns Invoice Id
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="taxAmount"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Guid CreateInvoice(decimal amount, decimal taxAmount, InvoiceType type)
		{
			var invoice = new Invoice
			{
				Amount = amount,
				TaxAmount = taxAmount,
				Type = type
			};

			invoiceRepository.Add(invoice);
			invoiceRepository.SaveInvoice(invoice);

			return invoice.Id;
		}

		public string InitialisePayment(Guid invoiceId, decimal amountPaid)
		{
			var invoice = invoiceRepository.GetInvoice(invoiceId);

			if (invoice == null) throw new InvalidOperationException( "Invoice not found." );

			var reference = Guid.NewGuid().ToString();
			var payment = new Payment
			{
				AmountPaid = amountPaid,
				Reference = reference,
			};
			
			payment.MarkAsInitialised();
			invoice.Payments.Add(payment);
			
			return reference;
		}

		public string ProcessPayment(string reference)
		{
			var invoice = invoiceRepository.GetInvoice(reference);
			
			if ( invoice == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			var payment = invoice.Payments.SingleOrDefault(x => x.Reference == reference);
			
			var responseMessage = string.Empty;

			var (success, message) = invoice.ProcessPayment(reference);

			if (success)
			{
				payment?.MarkAsPaid();
				responseMessage = message;
			}
			else
			{
				payment!.MarkAsDeclined();
				payment.Remarks = message;
				responseMessage = message;
			}

			invoiceRepository.Update(invoice);
			invoiceRepository.SaveInvoice(invoice);

			return responseMessage;
		}
	}
}