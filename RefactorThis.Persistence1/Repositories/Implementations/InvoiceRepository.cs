using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RefactorThis.Domain1.Models.Entities;
using RefactorThis.Domain1.Repositories.Contracts;
using RefactorThis.Domain1.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence1.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ILogger<InvoiceRepository> _logger;
        private readonly ICollection<Invoice> _invoices = new HashSet<Invoice>();
        private readonly IValidator<Invoice> _validator;
        public InvoiceRepository(ILogger<InvoiceRepository> logger)
        {
            _logger = logger;
        }
        //public sealed class Validator : AbstractValidator<Invoice>
        //{
        //    public Validator()
        //    {
        //        RuleFor(r => r.Amount).GreaterThan(0);
        //    }
        //}
        public async Task AddAsync(Invoice invoice)
        {
            try
            {
                //var validationResult  = await _validator.ValidateAsync(invoice);

                //if (validationResult != null) 
                //{
                //    throw new InvalidOperationException("Error");
                //}

                _logger.LogInformation($"InvoiceRepository | AddAsync - [Request - {JsonConvert.SerializeObject(invoice)}");

                await Task.Run(() => _invoices.Add(invoice));
            }
            catch (Exception ex)
            {
                _logger.LogError($"InvoiceRepository | AddAsync - [Exception] - {ex.Message}");
            }
        }

        public Task<Invoice?> GetInvoiceAsync(string reference)
        {
            try
            {
                _logger.LogInformation($"InvoiceRepository | AddAsync - [Request - {reference}");

                var result = _invoices.FirstOrDefault(_ => _.Payments == null
                          || _.Payments.Any(a => a.Reference.Equals(reference, StringComparison.CurrentCultureIgnoreCase)));

                if (result is null)
                {
                    return Task.FromResult(_invoices.FirstOrDefault());
                }
                else
                {
                    return Task.FromResult(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"InvoiceRepository | GetInvoiceAsync - [Exception] - {ex.Message}");
                throw;
            }
         
        }

        public async Task SaveInvoiceAsync(Invoice invoice)
        {
           await Task.CompletedTask;
        }
    }
}
