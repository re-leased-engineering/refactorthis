using System;
using NUnit.Framework;
using RefactorThis.Persistence;
using RefactorThis.Services;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			InvoiceService paymentProcessor = new InvoiceService(repo);
			const string paymentReference = "this should not exist";
			var failureMessage = "";

			try
			{
				_ = paymentProcessor.ProcessPayment(paymentReference);
			}
			catch ( InvalidOperationException e )
			{
				failureMessage = e.Message;
			}
			Assert.That("There is no invoice matching this payment", Is.EqualTo(failureMessage) );
			Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(paymentReference));
		}
		
		
		[TestCase(InvoiceType.Commercial)]
		[TestCase(InvoiceType.Standard)]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( InvoiceType type )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, type);
			var invoiceAmount = invoice.TotalAmount;
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, invoiceAmount);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice.Id, 12M);
		
			paymentProcessor.ProcessPayment(payment1Reference);
			var result = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That(result.Message, Is.EqualTo( "invoice was already fully paid") );
			Assert.That(false, Is.EqualTo(result.Success));
			Assert.That(invoiceAmount, Is.EqualTo(invoice.Amount + (invoice.Amount * invoice.TaxPercentage)));
		}

		[TestCase(InvoiceType.Commercial)]
		[TestCase(InvoiceType.Standard)]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded(InvoiceType type)
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(0M, type);
			var invoiceAmount = invoice.TotalAmount;
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, invoiceAmount);

			var result = paymentProcessor.ProcessPayment(payment1Reference);


			Assert.That(result.Message, Is.EqualTo("no payment needed"));
		}

		[TestCase(InvoiceType.Commercial)]
		[TestCase(InvoiceType.Standard)]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue(InvoiceType type)
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, type);
			var firstPayment = 50M;
			var secondPayment = 90M;
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, firstPayment);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice.Id, secondPayment);

			paymentProcessor.ProcessPayment(payment1Reference);
			var result = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That( result.Message, Is.EqualTo("the payment is greater than the partial amount remaining") );
			Assert.That(false, Is.EqualTo(result.Success));
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, 150M);

			var result = paymentProcessor.ProcessPayment(payment1Reference);
		
			Assert.That( result.Message, Is.EqualTo("the payment is greater than the invoice amount") );
			Assert.That(false, Is.EqualTo(result.Success));
			Assert.That(150M, Is.GreaterThan(invoice.TotalAmount));
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, InvoiceType.Commercial);
			var firstPayment = invoice.TotalAmount - 100M;
			var secondPayment = invoice.TotalAmount - firstPayment;
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, firstPayment);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice.Id, secondPayment);

			_ = paymentProcessor.ProcessPayment(payment1Reference);
			var result = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That(result.Message, Is.EqualTo("invoice is now fully paid")  );
			Assert.That(true, Is.EqualTo(result.Success));
			Assert.That(firstPayment + secondPayment, Is.EqualTo(invoice.TotalAmount));
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, 100M);

			var result1 = paymentProcessor.ProcessPayment(payment1Reference);

		
			Assert.That( "invoice is now partially paid", Is.EqualTo(result1.Message) );
			Assert.That(true, Is.EqualTo(result1.Success));
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice.Id, 100M);

			var result1 = paymentProcessor.ProcessPayment(payment1Reference);

			Assert.That(true, Is.EqualTo(result1.Success));
			Assert.That( "invoice is now partially paid", Is.EqualTo(result1.Message) );
		}
	}
}