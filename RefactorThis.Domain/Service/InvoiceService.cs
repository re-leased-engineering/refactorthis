using System;
using RefactorThis.Domain.Service;
using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Entities;

namespace RefactorThis.Domain
{
	public interface IInvoiceService
	{
		string ProcessPayment(Payment payment);
	}

	public class InvoiceService : IInvoiceService
	{
		private readonly IInvoiceRepository _invoiceRepository;
		private readonly IInvoiceValidationService _invoiceValidationService;

		private const decimal TaxAmount = 0.14m;

		public InvoiceService(IInvoiceRepository invoiceRepository, IInvoiceValidationService invoiceValidationService)
		{
			_invoiceRepository = invoiceRepository;
			_invoiceValidationService = invoiceValidationService;
		}

		//NOTE: STRINGS CAN BE TRANSFERRED TO A RESX FILES TO MINIMIZE HARDCODED IMPLEMENTATION AND CREATE A CENTRALIZED RESOURCE
		public string ProcessPayment(Payment payment)
		{
			var invoice = _invoiceRepository.GetInvoice(payment.Reference);

			if (invoice is null) throw new InvalidOperationException("There is no invoice matching this payment");

			if (_invoiceValidationService.InvoiceAmountIsZero(invoice.Amount))
			{
				if (!_invoiceValidationService.InvoiceHasExistingPaymentTransactions(invoice.Payments))
				{
					return "no payment needed";
				}

				throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
			}

			if (_invoiceValidationService.InvoiceHasExistingPaymentTransactions(invoice.Payments))
			{
				if (_invoiceValidationService.HasExistingPayments(invoice.Payments) && _invoiceValidationService.IsInvoiceFullyPaid(invoice.Amount, invoice.Payments))
				{
					return "invoice was already fully paid";
				}

				if (_invoiceValidationService.HasExistingPayments(invoice.Payments) && _invoiceValidationService.PaymentExceedsInvoiceAmount(payment.Amount, invoice.Amount, invoice.AmountPaid))
				{
					return "the payment is greater than the partial amount remaining";
				}

				bool isInvoiceFullyPaid = _invoiceValidationService.IsInvoiceFullyPaid(invoice.Amount, invoice.AmountPaid, payment.Amount);

				UpdateInvoice(payment, invoice);

				return isInvoiceFullyPaid ? "final partial payment received, invoice is now fully paid"
				: "another partial payment received, still not fully paid"; ;
			}

			if (payment.Amount > invoice.Amount)
			{
				return "the payment is greater than the invoice amount";
			}

			bool isFullyPaid = _invoiceValidationService.IsInvoiceFullyPaid(invoice.Amount, payment.Amount);

			UpdateInvoice(payment, invoice);

			return isFullyPaid ? "invoice is now fully paid"
				: "invoice is now partially paid"; ;
		}

		private void UpdateInvoice(Payment payment, Invoice inv)
		{
			switch (inv.Type)
			{
				case InvoiceType.Standard:
					inv.AmountPaid += payment.Amount;
					inv.Payments.Add(payment);
					break;
				case InvoiceType.Commercial:
					inv.AmountPaid += payment.Amount;
					inv.TaxAmount += payment.Amount * TaxAmount;
					inv.Payments.Add(payment);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			inv.Save();
		}
	}
}