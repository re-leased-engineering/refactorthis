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
		public InvoiceDto CreateInvoice(decimal amount, InvoiceType type)
		{
			var invoice = new Invoice
			{
				Amount = amount,
				Type = type
			};

			invoiceRepository.Add(invoice);
			invoiceRepository.SaveChanges();

			return new InvoiceDto
			{
				Amount = invoice.Amount,
				TaxAmount = invoice.Amount * invoice.GetTaxPercentage(),
				TotalAmount = invoice.GetTotalAmount(),
				TaxPercentage = invoice.GetTaxPercentage(),
				Id = invoice.Id,
				Type = invoice.Type,
				Payments = invoice.Payments.Select(x => new PaymentDto()
				{
					AmountPaid = x.AmountPaid,
					Reference = x.Reference,
					Remarks = x.Remarks,
					Status = x.Status
				})
			};
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

		public ProcessPaymentResponseDto ProcessPayment(string reference)
		{
			var invoice = invoiceRepository.GetInvoice(reference);
			
			if ( invoice == null )
			{
				throw new InvalidOperationException( "There is no invoice matching this payment" );
			}

			var payment = invoice.Payments.SingleOrDefault(x => x.Reference == reference);
			

			var (success, message) = invoice.ProcessPayment(reference);
			var responseMessage = new ProcessPaymentResponseDto(success, message);
			if (success)
			{
				payment?.MarkAsPaid();
			}
			else
			{
				payment!.MarkAsDeclined();
				payment.Remarks = responseMessage.Message;
			}

			invoiceRepository.Update(invoice);
			invoiceRepository.SaveChanges();

			return responseMessage;
		}
	}

	public record ProcessPaymentResponseDto(bool Success, string Message);
}