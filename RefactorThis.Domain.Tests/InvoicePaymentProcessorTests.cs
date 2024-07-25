using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
	[TestClass]
	public class InvoicePaymentProcessorTests
	{
		[TestMethod]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
		{
			var repo = new InvoiceRepository();

			Invoice invoice = null;
			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment();
			var failureMessage = "";

			try
			{
				var result = paymentProcessor.ProcessPayment(payment);
			}
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual("There is no invoice matching this payment", failureMessage);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
			var repo = new InvoiceRepository();

			var invoice = new Invoice(repo)
			{
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};

			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment();

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("no payment needed", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
			var repo = new InvoiceRepository();

			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 10,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 10
					}
				}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment();

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the partial amount remaining", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>()
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 6
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the invoice amount", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 5
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>() { new Payment() { Amount = 10 } }
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 10
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("another partial payment received, still not fully paid", result);
		}

		[TestMethod]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
			var repo = new InvoiceRepository();
			var invoice = new Invoice(repo)
			{
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>()
			};
			repo.Add(invoice);

			var paymentProcessor = new InvoiceService(repo);

			var payment = new Payment()
			{
				Amount = 1
			};

			var result = paymentProcessor.ProcessPayment(payment);

			Assert.AreEqual("invoice is now partially paid", result);
		}
	}
}