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
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
			//seems odd that we are handling no payment needed
			//- it's just another way of saying that there is no invoice matching the payment reference
			
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
		//
		// [Test]
		// public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		// {
		// 	var repo = new InvoiceRepository( );
		// 	var invoice = new Invoice( repo )
		// 	{
		// 		Amount = 5,
		// 		AmountPaid = 0,
		// 		Payments = new List<Payment>( )
		// 	};
		// 	repo.Add( invoice );
		//
		// 	var paymentProcessor = new InvoiceService( repo );
		//
		// 	var payment = new Payment( )
		// 	{
		// 		Amount = 6
		// 	};
		//
		// 	var result = paymentProcessor.ProcessPayment( payment );
		//
		// 	Assert.That( "the payment is greater than the invoice amount", Is.EqualTo(result) );
		// }
		//
		// [Test]
		// public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		// {
		// 	var repo = new InvoiceRepository( );
		// 	var invoice = new Invoice( repo )
		// 	{
		// 		Amount = 10,
		// 		AmountPaid = 5,
		// 		Payments = new List<Payment>
		// 		{
		// 			new Payment
		// 			{
		// 				Amount = 5
		// 			}
		// 		}
		// 	};
		// 	repo.Add( invoice );
		//
		// 	var paymentProcessor = new InvoiceService( repo );
		//
		// 	var payment = new Payment( )
		// 	{
		// 		Amount = 5
		// 	};
		//
		// 	var result = paymentProcessor.ProcessPayment( payment );
		//
		// 	Assert.That( "final partial payment received, invoice is now fully paid", Is.EqualTo(result) );
		// }
		//
		// [Test]
		// public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		// {
		// 	var repo = new InvoiceRepository( );
		// 	var invoice = new Invoice( repo )
		// 	{
		// 		Amount = 10,
		// 		AmountPaid = 0,
		// 		Payments = new List<Payment>( ) { new Payment( ) { Amount = 10 } }
		// 	};
		// 	repo.Add( invoice );
		//
		// 	var paymentProcessor = new InvoiceService( repo );
		//
		// 	var payment = new Payment( )
		// 	{
		// 		Amount = 10
		// 	};
		//
		// 	var result = paymentProcessor.ProcessPayment( payment );
		//
		// 	Assert.That( "invoice was already fully paid", Is.EqualTo(result) );
		// }
		//
		// [Test]
		// public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		// {
		// 	var repo = new InvoiceRepository( );
		// 	var invoice = new Invoice( repo )
		// 	{
		// 		Amount = 10,
		// 		AmountPaid = 5,
		// 		Payments = new List<Payment>
		// 		{
		// 			new Payment
		// 			{
		// 				Amount = 5
		// 			}
		// 		}
		// 	};
		// 	repo.Add( invoice );
		//
		// 	var paymentProcessor = new InvoiceService( repo );
		//
		// 	var payment = new Payment( )
		// 	{
		// 		Amount = 1
		// 	};
		//
		// 	var result = paymentProcessor.ProcessPayment( payment );
		//
		// 	Assert.That( "another partial payment received, still not fully paid", Is.EqualTo(result) );
		// }
		//
		// [Test]
		// public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		// {
		// 	var repo = new InvoiceRepository( );
		// 	var invoice = new Invoice( repo )
		// 	{
		// 		Amount = 10,
		// 		AmountPaid = 0,
		// 		Payments = new List<Payment>( )
		// 	};
		// 	repo.Add( invoice );
		//
		// 	var paymentProcessor = new InvoiceService( repo );
		//
		// 	var payment = new Payment( )
		// 	{
		// 		Amount = 1
		// 	};
		//
		// 	var result = paymentProcessor.ProcessPayment( payment );
		//
		// 	Assert.That( "invoice is now partially paid", Is.EqualTo(result) );
		// }
	}
}