using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RefactorThis.Domain.Models.Entities;
using RefactorThis.Domain.Repositories.Contracts;
using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Persistence.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ILogger<InvoiceRepository> _logger;
        private readonly ICollection<Invoice> _invoices = new HashSet<Invoice>();
        public InvoiceRepository(ILogger<InvoiceRepository> logger)
        {
            _logger = logger;
        }
        public async Task AddAsync(Invoice invoice)
        {
            try
            {
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
                _logger.LogInformation($"InvoiceRepository | GetInvoiceAsync - [Request - {reference}");

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
