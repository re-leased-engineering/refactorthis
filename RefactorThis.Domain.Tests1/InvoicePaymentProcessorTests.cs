using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using RefactorThis.Domain1.Services.Contracts;
using RefactorThis.Domain1.Services.Implementations;
using RefactorThis.Persistence1.Repositories.Implementations;

namespace RefactorThis.Domain.Tests1
{
    [TestClass]
    public class InvoicePaymentProcessorTests
    {
        protected IInvoiceRepository? _invoiceRepository;
        protected IInvoiceService? _invoiceService;
        protected ILoggerFactory? _loggerFactory;
        [TestInitialize]
        public void Initialize()
        {
            var invoiceServiceLoggerMock = new Mock<ILogger<InvoiceService>>();
            var invoiceRepositoryLoggerMock = new Mock<ILogger<InvoiceRepository>>();
            _invoiceRepository = new InvoiceRepository(invoiceRepositoryLoggerMock.Object);
            _invoiceService = new InvoiceService(_invoiceRepository, invoiceServiceLoggerMock.Object);

        }

        [TestMethod]
        public async Task ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            var payment = new Payment();
            var failureMessage = "";
            try
            {
                if (_invoiceService == null) throw new NullReferenceException();

                await _invoiceService.ProcessPaymentAsync(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null
            };

            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment();

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("no payment needed", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        Amount = 10,
                    }
                }
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment();

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
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
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);
            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 5,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 6
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();
            var invoice = new Invoice(_invoiceRepository)
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
            await _invoiceRepository.AddAsync(invoice);


            var payment = new Payment()
            {
                Amount = 5
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } }
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [TestMethod]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();

            var invoice = new Invoice(_invoiceRepository)
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
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();
            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 1
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }
        [TestMethod]
        public async Task ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPaymentExistsAndAmountPaidIsEqualToInvoiceAmount()
        {
            if (_invoiceRepository == null) throw new NullReferenceException();
            if (_invoiceService == null) throw new NullReferenceException();
            var invoice = new Invoice(_invoiceRepository)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>()
            };
            await _invoiceRepository.AddAsync(invoice);

            var payment = new Payment()
            {
                Amount = 10
            };

            var result = await _invoiceService.ProcessPaymentAsync(payment);

            Assert.AreEqual("invoice is now fully paid", result);
        }
    }
}