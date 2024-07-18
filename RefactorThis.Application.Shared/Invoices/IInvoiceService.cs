using RefactorThis.Application.Shared.Invoices.DTOs;

namespace RefactorThis.Application.Shared.Invoices;

public interface IInvoiceService
{
    Task<int> AddAsync(CreateInvoiceDto invoiceDto);
}