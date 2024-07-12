using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RefactorThis.Domain.Enums;
using RefactorThis.Domain.Models.Entities;
using RefactorThis.Domain.Repositories.Contracts;
using RefactorThis.Domain.Services.Contracts;
using RefactorThis.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ILogger<InvoiceService> _logger;
        private readonly IInvoiceRepository _invoiceRepository;
        private const decimal TaxRate = 0.14m;
        public InvoiceService(IInvoiceRepository invoiceRepository, ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }
        public async Task<string> ProcessPaymentAsync(Payment payment)
        {
            var inv = await _invoiceRepository.GetInvoiceAsync(payment.Reference);

            var responseMessage = string.Empty;

            _logger.LogInformation($"InvoiceService | ProcessPaymentAsync -  {JsonConvert.SerializeObject(payment)}");

            if (inv is null)
            {
                _logger.LogError($"InvoiceService | ProcessPaymentAsync -  {JsonConvert.SerializeObject(payment)}");

                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            //check the payment validity
            var validationMessage = PaymentValidators.CheckPaymentValidity(inv, payment);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                return validationMessage;
            }

            responseMessage = ProcessInvoicePayment(inv, payment);

            await _invoiceRepository.SaveInvoiceAsync(inv);

            return responseMessage;
        }
        private string ProcessInvoicePayment(Invoice inv, Payment payment)
        {
            string validationMessage = string.Empty;

            //Check if has Partial Payment
            if (inv.Payments != null && inv.Payments.Any())
            {
                validationMessage = PaymentValidators.CheckIfFinalPartialPayment(inv, payment);
                inv.AmountPaid += payment.Amount;
                if (inv.Type == InvoiceType.Commercial)
                {
                    inv.TaxAmount += payment.Amount * TaxRate;
                }

            }
            else
            {
                //No payment exist 
                validationMessage = PaymentValidators.CheckIfInvoiceIsFullyPaid(inv, payment);
                inv.AmountPaid = payment.Amount;
                inv.TaxAmount = payment.Amount * TaxRate;
            }
            inv.Payments?.Add(payment);
            return validationMessage;
        }
    }
}
