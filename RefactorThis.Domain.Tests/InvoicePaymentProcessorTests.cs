using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using RefactorThis.Domain.Service;
using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Entities;
using RefactorThis.Persistence.Implementations;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
	public class InvoicePaymentProcessorTests
	{
		private IInvoiceRepository _invoiceRepository;
		private IInvoiceService _invoiceService;
		private IInvoiceValidationService _invoiceValidationService;

		[SetUp]
		public void Setup()
		{
			var invoiceRepositoryMock = new Mock<InvoiceRepository>();
			_invoiceRepository = invoiceRepositoryMock.Object;
			
			var invoiceValidationMock = new Mock<InvoiceValidationService>();
			_invoiceValidationService = invoiceValidationMock.Object;

			var invoiceServiceMock = new Mock<InvoiceService>(_invoiceRepository, _invoiceValidationService);
			_invoiceService = invoiceServiceMock.Object;
		}

		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
		{			
			var payment = new Payment();
			var failureMessage = "";

			try
			{
				var result = _invoiceService.ProcessPayment(payment);
			}
			catch (InvalidOperationException e)
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual("There is no invoice matching this payment", failureMessage);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 0,
				AmountPaid = 0,
				Reference = "ACS123",
				Payments = null
			};

			_invoiceRepository.Add(invoice);
			var payment = new Payment { Reference = "ACS123" };
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("no payment needed", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 10,
				Reference = "ADV233",
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 10,
						Reference = "ADV233"
					}
				}
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment { Reference = "ADV233" };
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 5,
				Reference = "AD1233",
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5,
						Reference = "AD1233"
					}
				}
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 6,
				Reference = "AD1233"
			};

			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the partial amount remaining", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 5,
				AmountPaid = 0,
				Reference = "AZN613",
				Payments = new List<Payment>()
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 6,
				Reference = "AZN613"
			};
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("the payment is greater than the invoice amount", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 5,
				Reference = "ADV233",
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5,
						Reference = "ADV233"

					}
				}
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 5,
				Reference = "ADV233"
			};
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 0,
				Reference = "ADV233",
				Payments = new List<Payment>() { new Payment() { Amount = 10 , Reference = "ADV233" } }
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 10,
				Reference = "ADV233"
			};
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("invoice was already fully paid", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5,
						Reference = "ADV233"
					}
				}
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 1,
				Reference = "ADV233"
			};

			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("another partial payment received, still not fully paid", result);
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
		{
			var invoice = new Invoice(_invoiceRepository)
			{
				Amount = 10,
				AmountPaid = 0,
				Reference = "ADV233",
				Payments = new List<Payment>()
			};
			_invoiceRepository.Add(invoice);

			var payment = new Payment()
			{
				Amount = 1,
				Reference = "ADV233",
			};
			var result = _invoiceService.ProcessPayment(payment);

			Assert.AreEqual("invoice is now partially paid", result);
		}
	}
}