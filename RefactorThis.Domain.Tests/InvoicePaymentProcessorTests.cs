using Microsoft.Extensions.Logging;
using Moq;
using RefactorThis.Application.Services;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Errors;
using RefactorThis.Domain.Repositories;
using RefactorThis.Domain.Services;
using RefactorThis.Domain.Shared;
using RefactorThis.Persistence.Repositories;
namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        private readonly Mock<ILogger<InvoiceService>> _invoiceServiceLoggerMock;
        public InvoicePaymentProcessorTests()
        {
            _invoiceServiceLoggerMock = new Mock<ILogger<InvoiceService>>();

        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            //arrange
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);
            var payment = new Payment();

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.NoInvoiceFound));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            //arrage
            var invoice = new Invoice()
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference="test" };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.NoPaymentNeeded));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded_ButHasPayment()
        {
            //arrange
            var invoice = new Invoice()
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = new List<Payment> { new Payment { Amount=10} }
            };

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test" };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.InvoiceInvalidState));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            //arrange
            var invoice = new Invoice()
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

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test" };

            //act
            var result = paymentProcessor.ProcessPayment(payment);

            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.InvoiceAlreadyFullPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            //arrange
            var invoice = new Invoice()
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
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test",Amount=6 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.PaymentIsGreaterthanPartialAmountRemaining));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            //arrange
            var invoice = new Invoice()
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 6 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);

            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.PaymentIsGreaterthanInvoiceAmount));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            //arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } }
            };

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 10 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(InvoiceErrors.InvoiceAlreadyFullPaid));
        }


        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue_Standard()
        {
            //arrange
            var repo = new InvoiceRepository();
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                },
                Type = Enums.InvoiceType.Standard
            };
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 5 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(App.PartialInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue_Commercial()
        {
            //arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 5
                    }
                },
                Type=Enums.InvoiceType.Commercial
            };
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 5 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(App.PartialInvoiceFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            //arrange
            var invoice = new Invoice()
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

            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 1 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);


            //assert
            Assert.That(result.Message, Is.EqualTo(App.PartialInvoiceNotFullyPaid));
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            //arrange
            var invoice = new Invoice()
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            invoiceRepositoryMock.Setup(m => m.GetInvoice("test")).Returns(invoice);


            var paymentProcessor = new InvoiceService(invoiceRepositoryMock.Object, _invoiceServiceLoggerMock.Object);

            var payment = new Payment() { Reference = "test", Amount = 1 };

            //act
            var result = paymentProcessor.ProcessPayment(payment);

            //assert
            Assert.That(result.Message, Is.EqualTo(App.PartialInvoicePaid));
        }
    }
}