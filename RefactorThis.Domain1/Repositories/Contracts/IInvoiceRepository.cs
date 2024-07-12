using FluentValidation;
using RefactorThis.Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Repositories.Contracts
{
    public interface IInvoiceRepository
    {
        Task SaveInvoiceAsync(Invoice invoice);
        Task<Invoice?> GetInvoiceAsync(string reference);
        Task AddAsync(Invoice invoice);

    }
}
