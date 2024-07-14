using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Invoices
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetInvoice(string reference, CancellationToken cancellationToken = default);

        void Add(Invoice invoice);
    }
}
