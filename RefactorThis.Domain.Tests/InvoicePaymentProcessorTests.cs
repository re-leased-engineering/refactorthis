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
		}
		
		
		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 112M);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice, 12M);
		
			paymentProcessor.ProcessPayment(payment1Reference);
			var result = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That( "invoice was already fully paid", Is.EqualTo(result) );
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 110M);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice, 12M);

			paymentProcessor.ProcessPayment(payment1Reference);
			var result = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That( "the payment is greater than the partial amount remaining", Is.EqualTo(result) );
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 150M);

			var result = paymentProcessor.ProcessPayment(payment1Reference);
		
			Assert.That( "the payment is greater than the invoice amount", Is.EqualTo(result) );
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 100M);
			var payment2Reference = paymentProcessor.InitialisePayment(invoice, 12M);

			var result1 = paymentProcessor.ProcessPayment(payment1Reference);
			var result2 = paymentProcessor.ProcessPayment(payment2Reference);
		
			Assert.That( "final partial payment received, invoice is now fully paid", Is.EqualTo(result2) );
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 100M);

			var result1 = paymentProcessor.ProcessPayment(payment1Reference);

		
			Assert.That( "invoice is now partially paid", Is.EqualTo(result1) );
		}
		
		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			IInvoiceRepository repo = new InvoiceRepository();
			var paymentProcessor = new InvoiceService(repo);
			var invoice = paymentProcessor.CreateInvoice(100M, 12M, InvoiceType.Commercial);
			var payment1Reference = paymentProcessor.InitialisePayment(invoice, 100M);

			var result1 = paymentProcessor.ProcessPayment(payment1Reference);

		
			Assert.That( "invoice is now partially paid", Is.EqualTo(result1) );
		}
	}
}