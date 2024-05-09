using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RefactorThis.Domain.Interface;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Interface;
using Microsoft.Extensions.Logging; // Used this package for logging purposes and to improve debuggability.
using RefactorThis.Persistence.Enums;

namespace RefactorThis.Domain
{
    //Register this in Dependency Injection Service
    public class InvoiceService : IInvoiceService
    {
		private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IInvoiceRepository invoiceRepository, ILogger<InvoiceService> logger)
		{
			_invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<string> ProcessPayment(Payment payment, CancellationToken cancellationToken = default)
        {
            try
            {
                var invoice = await GetInvoice(payment.Reference, cancellationToken);

                var responseMessage = string.Empty;

                if (invoice == null)
                {
                    string invoiceNullErrorMessage = "There is no invoice matching this payment"; // create variable instead of repeatitive string message

                    LogError(invoiceNullErrorMessage);

                    throw new InvalidOperationException(invoiceNullErrorMessage);
                }

                if (invoice.Amount == 0)
                {
                    if (invoice.Payments == null || !invoice.Payments.Any())
                    {
                        responseMessage = "No payment needed";
                    }
                    else
                    {
                        string invalidInvoiceErrorMessage = "The invoice is in an invalid state: it has an amount of 0 and it has payments."; // create variable instead of repeatitive string message

                        LogError(invalidInvoiceErrorMessage);

                        throw new InvalidOperationException(invalidInvoiceErrorMessage);
                    }
                }
                else
                {
                    if (invoice.Payments != null && invoice.Payments.Any())
                    {
                        HandleExistingPayments(invoice, payment, ref responseMessage);
                    }
                    else
                    {
                        HandleNewPayment(invoice, payment, ref responseMessage);
                    }
                }

                
                await SaveInvoice(invoice, cancellationToken);

                return responseMessage;
            }
            catch (Exception ex)
            {
                LogError($"Method:[ProcessPayment] An error occurred while processing payment: {ex.Message}, stackTrace: {ex.StackTrace}");

                throw ex;
            }
        }

        public async Task<Invoice> GetInvoice(string reference, CancellationToken cancellationToken)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoice(reference, cancellationToken);

                return invoice;
            }
            catch (Exception ex)
            {
                LogError($"Method:[GetInvoice] An error occurred while fetching invoice: {ex.Message}, stackTrace: {ex.StackTrace}");

                return null;
            }
        }

        public async Task SaveInvoice(Invoice invoice, CancellationToken cancellationToken)
        {
            try
            {
                await _invoiceRepository.SaveInvoice(invoice, cancellationToken);
            }
            catch (Exception ex)
            {
                LogError($"Method:[SaveInvoice] An error occurred while saving invoice: {ex.Message}, stackTrace: {ex.StackTrace}");
                throw ex;
            }
        }

        //Method to add invoice payments
        public void CalculateTax(Invoice invoice, Payment payment)
        {
            if (invoice.Type == InvoiceType.Commercial)
            {
                invoice.TaxAmount += payment.Amount * 0.14m;
            }
        }

        private void HandleExistingPayments(Invoice invoice, Payment payment, ref string responseMessage)
        {
            try
            {
                var invoicePaymentSum = invoice.Payments.Sum(x => x.Amount);

                if (invoicePaymentSum != 0 && invoice.Amount == invoicePaymentSum)
                {
                    responseMessage = "Invoice was already fully paid";
                }
                else if (invoicePaymentSum != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
                {
                    responseMessage = "The payment is greater than the partial amount remaining";
                }
                else
                {
                    if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
                    {
                        ProcessFinalPartialPayment(invoice, payment, ref responseMessage);
                    }
                    else
                    {
                        ProcessAnotherPartialPayment(invoice, payment, ref responseMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Method:[HandleExistingPayments] An error occurred while handling existing payments: {ex.Message}, stackTrace: {ex.StackTrace}");
            }
        }

        private void HandleNewPayment(Invoice invoice, Payment payment, ref string responseMessage)
        {
            try
            {
                if (payment.Amount > invoice.Amount)
                {
                    responseMessage = "The payment is greater than the invoice amount";
                }
                else if (invoice.Amount == payment.Amount)
                {
                    ProcessFullPayment(invoice, payment, ref responseMessage);
                }
                else
                {
                    ProcessPartialPayment(invoice, payment, ref responseMessage);
                }
            }
            catch (Exception ex)
            {
                LogError($"Method:[HandleNewPayment] An error occurred while handling new payment: {ex.Message}, stackTrace: {ex.StackTrace}");
            }
        }

        //Final partial payment processor
        private void ProcessFinalPartialPayment(Invoice invoice, Payment payment, ref string responseMessage)
        {
            invoice.AmountPaid += payment.Amount;
            AddPayment(invoice, payment);
            responseMessage = "Final partial payment received, invoice is now fully paid";
        }

        //Another partial payment processor
        private void ProcessAnotherPartialPayment(Invoice invoice, Payment payment, ref string responseMessage)
        {
            AddPayment(invoice, payment);
            responseMessage = "Another partial payment received, still not fully paid";
        }

        //Full payment processor
        private void ProcessFullPayment(Invoice invoice, Payment payment, ref string responseMessage)
        {
            invoice.AmountPaid = payment.Amount;
            CalculateTax(invoice, payment);
            AddPayment(invoice, payment);
            responseMessage = "Invoice is now fully paid";
        }

        //Partial payment processor
        private void ProcessPartialPayment(Invoice invoice, Payment payment, ref string responseMessage)
        {
            invoice.AmountPaid = payment.Amount;
            CalculateTax(invoice, payment);
            AddPayment(invoice, payment);
            responseMessage = "Invoice is now partially paid";
        }

        //Method to add invoice payments
        private void AddPayment(Invoice invoice, Payment payment)
        {
            invoice.Payments.Add(payment);
        }

        //Method for adding error logs.
        //For now it's isolated for payment processing but it's recommended to make it global and re-usable
        private void LogError(string errorMessage)
        {
            _logger.LogError(errorMessage);
        }
    }
}