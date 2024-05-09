using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq; //Moq is a popular mocking library for .NET which allows you to easily create mock objects for interfaces or classes.
//using NUnit.Framework; ----- Sorry I commented this and did not Nunit test because I cannot run it in my local machine
using RefactorThis.Persistence;
using RefactorThis.Persistence.Enums;
using RefactorThis.Persistence.Interface;
using Xunit; // I used unit test so I can run my test cases

namespace RefactorThis.Domain.Tests
{
	public class InvoicePaymentProcessorTests
	{
        [Fact]
        public async Task ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            // Arrange
            var mockInvoiceRepository = new Mock<IInvoiceRepository>();
            mockInvoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync((Invoice)null); // Mock returning null for any reference

            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                invoiceService.ProcessPayment(new Payment { Reference = "PAY001" })
            );

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            // Arrange
            var mockInvoiceRepository = new Mock<IInvoiceRepository>();

            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            var payment = new Payment
            {
                Reference = "PAY001",
                Amount = 0 // Payment amount is set to 0
            };

            // Mock the GetInvoice method to return an invoice with amount 0
            mockInvoiceRepository.Setup(repo => repo.GetInvoice(payment.Reference, CancellationToken.None))
                .ReturnsAsync(new Invoice
                {
                    AmountPaid = 0,
                    Amount = 0,
                    Payments = null
                });

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("No payment needed", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            // Arrange
            var mockRepository = new Mock<IInvoiceRepository>();
            var mockLogger = new Mock<ILogger<InvoiceService>>();

            // Creating a sample fully paid invoice for the test
            var paymentList = new List<Payment>
            {
                new Payment
                {
                    Amount = 10
                }
            };

            var fullyPaidInvoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = paymentList
            };

            // Setting up mock behavior for the repository
            mockRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fullyPaidInvoice);

            // Creating the invoice service instance with mocked dependencies
            var invoiceService = new InvoiceService(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await invoiceService.ProcessPayment(new Payment { Reference = "PAY001", Amount = 50 });

            // Assert
            Assert.Equal("Invoice was already fully paid", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // Arrange
            var reference = "PAY001";
            var payment = new Payment { Reference = reference, Amount = 6 };

            var invoice = new Invoice { 
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

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            invoiceRepositoryMock.Setup(repo => repo.GetInvoice(reference, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

            var loggerMock = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(invoiceRepositoryMock.Object, loggerMock.Object);

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("The payment is greater than the partial amount remaining", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // Arrange
            var mockInvoiceRepository = new Mock<IInvoiceRepository>();
            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            var payment = new Payment
            {
                Reference = "PAY001",
                Amount = 150 // Exceeds invoice amount
            };

            var invoice = new Invoice
            {
                Amount = 100,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            mockInvoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("The payment is greater than the invoice amount", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            // Arrange
            var mockInvoiceRepository = new Mock<IInvoiceRepository>();
            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            var payment = new Payment
            {
                Reference = "PAY001",
                Amount = 5 // Partial payment
            };

            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 5, // Partial payment already made
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                }
            };

            mockInvoiceRepository.Setup(repo => repo.GetInvoice(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("Final partial payment received, invoice is now fully paid", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            // Arrange
            var reference = "PAY001";

            var payment = new Payment { 
                Reference = reference, 
                Amount = 10 
            };

            var invoice = new Invoice { 
                Amount = 10, 
                AmountPaid = 0,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10
                    }
                }
            };


            var mockInvoiceRepository = new Mock<IInvoiceRepository>();

            mockInvoiceRepository.Setup(repo => repo.GetInvoice(reference, default(CancellationToken))).ReturnsAsync(invoice);

            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("Invoice was already fully paid", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            // Arrange
            var mockInvoiceRepository = new Mock<IInvoiceRepository>();
            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var payment = new Payment
            {
                Reference = "PAY001",
                Amount = 1
            };

            var invoice = new Invoice
            {
                Amount = 10, 
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment { Amount = 5 }
                }
            };

            mockInvoiceRepository.Setup(r => r.GetInvoice(It.IsAny<string>(), default))
                                 .ReturnsAsync(invoice);

            var invoiceService = new InvoiceService(mockInvoiceRepository.Object, mockLogger.Object);

            // Act
            var result = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("Another partial payment received, still not fully paid", result);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var reference = "PAY001";

            var payment = new Payment
            {
                Reference = reference,
                Amount = 1
            };
            var invoice = new Invoice
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            var mockRepository = new Mock<IInvoiceRepository>();
            mockRepository.Setup(repo => repo.GetInvoice(reference, CancellationToken.None))
                .ReturnsAsync(invoice);

            var mockLogger = new Mock<ILogger<InvoiceService>>();

            var invoiceService = new InvoiceService(mockRepository.Object, mockLogger.Object);

            // Act
            var response = await invoiceService.ProcessPayment(payment);

            // Assert
            Assert.Equal("Invoice is now partially paid", response);
        }
}