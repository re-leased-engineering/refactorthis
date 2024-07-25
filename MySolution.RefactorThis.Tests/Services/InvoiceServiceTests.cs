using Moq;
using MySolution.RefactorThis.Domain.Models;
using MySolution.RefactorThis.Domain.Repositories.Contracts;
using MySolution.RefactorThis.Domain.Services.Implementations;

namespace MySolution.RefactorThis.Tests.Services
{
    [TestClass]
    public class InvoiceServiceTests
    {
        private Mock<IInvoiceRepository> InvoiceRepositoryMock { get; set; } = new Mock<IInvoiceRepository>();

        [TestMethod]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

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
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceIsInvalid()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 0,
                    AmountPaid = 0,
                    Payments = new List<Payment> { 
                        new Payment(){ 
                            Amount = 1, 
                            Reference = "Test"
                        }
                    }
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

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
            Assert.AreEqual("The invoice is in an invalid state, it has an amount of 0 and it has payments.", failureMessage);

        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 0,
                    AmountPaid = 0,
                    Payments = null
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("no payment needed", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
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
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
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
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
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
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnNotFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidLessThanAmountDue()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 10,
                    AmountPaid = 4,
                    Payments = new List<Payment>
                    {
                        new Payment
                        {
                            Amount = 5
                        }
                    }
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 5
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 5,
                    AmountPaid = 0,
                    Payments = new List<Payment>()
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 10,
                    AmountPaid = 0,
                    Payments = new List<Payment>() { }
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice is now fully paid", result);
        }

        [TestMethod]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            InvoiceRepositoryMock.Setup(x => x.GetInvoice(It.IsAny<string>()))
                .Returns(new Invoice()
                {
                    Amount = 10,
                    AmountPaid = 0,
                    Payments = new List<Payment>()
                });

            var paymentProcessor = new InvoiceService(InvoiceRepositoryMock.Object);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }
    }
}
