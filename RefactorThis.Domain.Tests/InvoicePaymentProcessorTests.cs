using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using RefactorThis.Domain.Abstractions;
using RefactorThis.Domain.Application.ProcessPayment;
using RefactorThis.Domain.Invoices;
using RefactorThis.Domain.Payments;
using Xunit;

namespace RefactorThis.Domain.Tests
{
    public class InvoicePaymentProcessorTests
    {      
        private readonly ProcessPaymentCommandHandler _handler;
        private readonly IInvoiceRepository _invoiceRepositoryMock;
        private readonly IUnitOfWork _unitOfWorkMock;

        public InvoicePaymentProcessorTests()
        {
            _invoiceRepositoryMock = Substitute.For<IInvoiceRepository>();
            _unitOfWorkMock = Substitute.For<IUnitOfWork>();

            _handler = new ProcessPaymentCommandHandler(
                _invoiceRepositoryMock,
                _unitOfWorkMock);
        }

        [Fact]
        public async Task ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            // Arrange
            var paymentCommand = new ProcessPaymentCommand(0, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns((Invoice)null);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.NotFound);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  0,
                  0,
                  0,
                  null,
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(0, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.NoPaymentNeeded);          
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            // Arrange        
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  10,
                  0,
                  new List<Payment> { Payment.Create(string.Empty, 10) },
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(0, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.AlreadyFullyPaid);                 
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  5,
                  0,
                  new List<Payment> { Payment.Create(string.Empty, 5) },
                  InvoiceType.Standard
              );

              var paymentCommand = new ProcessPaymentCommand(6, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.PaymentExceedsRemainingAmount);           
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  5,
                  0,
                  0,
                  new List<Payment>(),
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(6, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.PaymentExceedsInvoiceAmount);
          
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  5,
                  0,
                  new List<Payment> { Payment.Create(string.Empty, 5) },
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(5, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);
          
            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(ResponseMessages.FinalPartialPaymentReceived);           
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  0,
                  0,
                  new List<Payment> { Payment.Create(string.Empty, 10) },
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(10, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.Error.Should().Be(InvoiceErrors.AlreadyFullyPaid);          
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  5,
                  0,
                  new List<Payment> { Payment.Create(string.Empty, 5) },
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(1, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(ResponseMessages.AnotherPartialPaymentReceived);
        }

        [Fact]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            // Arrange
            var invoice = Invoice.Create(
                  Guid.NewGuid(),
                  10,
                  0,
                  0,
                  new List<Payment>(),
                  InvoiceType.Standard
              );

            var paymentCommand = new ProcessPaymentCommand(1, string.Empty);

            _invoiceRepositoryMock
                .GetInvoice(paymentCommand.Reference, Arg.Any<CancellationToken>())
                .Returns(invoice);

            // Act
            Result<string> result = await _handler.Handle(paymentCommand, default);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(ResponseMessages.InvoiceNowPartiallyPaid);           
        }


    }
}