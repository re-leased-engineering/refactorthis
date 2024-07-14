using RefactorThis.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Invoices
{
    public static class ResponseMessages
    {
        public const string FinalPartialPaymentReceived = "Final partial payment received, invoice is now fully paid.";
        public const string AnotherPartialPaymentReceived = "Another partial payment received, still not fully paid.";
        public const string InvoiceNowFullyPaid = "Invoice is now fully paid.";
        public const string InvoiceNowPartiallyPaid = "Invoice is now partially paid.";           
    }
}
