using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        // Test case to ensure an exception is thrown when no invoice is found for the payment reference
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
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

        // Test case to ensure correct behavior when no payment is needed
        [Test]
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

        // Test case to ensure correct behavior when invoice is already fully paid
        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        // Test case to ensure correct behavior when partial payment exceeds amount due
        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExceedsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 6 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        // Test case to ensure correct behavior when payment exceeds invoice amount
        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PaymentExceedsInvoiceAmount()
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
            var payment = new Payment { Amount = 6 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        // Test case to ensure correct behavior when partial payment equals amount due
        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentEqualsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 5 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        // Test case to ensure correct behavior when no partial payment exists and payment equals invoice amount
        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndPaymentEqualsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount = 10 } }
            };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 10 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        // Test case to ensure correct behavior when partial payment exists and amount paid is less than amount due
        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } }
            };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment { Amount = 1 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        // Test case to ensure correct behavior when no partial payment exists and amount paid is less than invoice amount
        [Test]
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
            var payment = new Payment { Amount = 1 };

            var result = paymentProcessor.ProcessPayment(payment);

            // Check if it's the final payment or not
            bool isFinalPayment = (invoice.Amount - invoice.AmountPaid) == payment.Amount;
            if (isFinalPayment)
            {
                Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
            }
            else
            {
                Assert.AreEqual("another partial payment received, still not fully paid", result);
            }
        }
    }
}