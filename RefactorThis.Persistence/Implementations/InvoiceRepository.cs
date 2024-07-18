using RefactorThis.Persistence.Contracts;
using RefactorThis.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ICollection<Invoice> _invoices;

        //ADD LOGGING AND ERROR HANDLING
        public InvoiceRepository()
        {
            _invoices = new List<Invoice>();
        }

        public Invoice GetInvoice(string reference)
        {
            try
            {
                if (reference == null)
                {
                    throw new ArgumentException($"No invoice payment with {reference} is found");
                }

                return _invoices.Where(invoice => invoice.Reference == reference || invoice.Payments.Any(payment => payment.Reference == reference))
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveInvoice(Invoice invoice)
        {
            //saves the invoice to the database
        }

        public void Add(Invoice invoice)
        {
            _invoices.Add(invoice);
        }
    }
}