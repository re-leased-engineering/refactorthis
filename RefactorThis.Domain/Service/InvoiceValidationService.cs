using RefactorThis.Persistence.Entities;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Domain.Service
{
	public interface IInvoiceValidationService
	{
		bool InvoiceAmountIsZero(decimal amount);
		bool IsInvoiceFullyPaid(decimal invoiceAmount, decimal invoiceAmountPaid, decimal paymentAmount);
		bool PaymentExceedsInvoiceAmount(decimal paymentAmount, decimal invoiceAmount, decimal invoiceAmountPaid);
		bool InvoiceHasExistingPaymentTransactions(List<Payment> payments);
		bool IsInvoiceFullyPaid(decimal invoiceAmount, List<Payment> payments);
		bool IsInvoiceFullyPaid(decimal invoiceAmount, decimal paymentAmount);
		bool HasExistingPayments(List<Payment> payments);
	}

	public class InvoiceValidationService : IInvoiceValidationService
	{
		public bool InvoiceAmountIsZero(decimal amount)
		{
			return amount == 0;
		}

		public bool PaymentExceedsInvoiceAmount(decimal paymentAmount, decimal invoiceAmount, decimal invoiceAmountPaid)
		{
			return paymentAmount > (invoiceAmount - invoiceAmountPaid);
		}

		public bool InvoiceHasExistingPaymentTransactions(List<Payment> payments)
		{
			return payments != null && payments.Any();
		}

		public bool IsInvoiceFullyPaid(decimal invoiceAmount, List<Payment> payments)
		{
			return invoiceAmount == payments.Sum(x => x.Amount);
		}

		public bool IsInvoiceFullyPaid(decimal invoiceAmount, decimal invoiceAmountPaid, decimal paymentAmount)
		{
			return (invoiceAmount - invoiceAmountPaid) == paymentAmount;
		}

		public bool IsInvoiceFullyPaid(decimal invoiceAmount, decimal paymentAmount)
		{
			return invoiceAmount == paymentAmount;
		}

		public bool HasExistingPayments(List<Payment> payments)
		{
			return payments.Sum(x => x.Amount) != 0;
		}
	}
}
