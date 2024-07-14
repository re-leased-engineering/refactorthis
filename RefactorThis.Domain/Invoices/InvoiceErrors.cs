using RefactorThis.Domain.Abstractions;

namespace RefactorThis.Domain.Invoices
{
    public static class InvoiceErrors
    {
        public static readonly Error NotFound = new Error(
            "Invoice.NotFound",
            "There is no invoice matching this payment");

        public static readonly Error NoPaymentNeeded = new Error(
           "Invoice.NoPaymentNeeded",
           "no payment needed");

        public static readonly Error InvalidState = new Error(
             "Invoice.InvalidState",
             "The invoice is in an invalid state, it has an amount of 0 and it has payments.");
     
        public static readonly Error PaymentExceedsInvoiceAmount = new Error(
           "Invoice.PaymentExceedsInvoiceAmount",
           "The payment is greater than the invoice amount.");

        public static readonly Error FinalPartialPaymentReceived = new Error(
           "Invoice.FinalPartialPaymentReceived",
           "Final partial payment received, invoice is now fully paid");

        public static readonly Error AlreadyFullyPaid = new Error(
           "Invoice.AlreadyFullyPaid",
           "The invoice was already fully paid.");

        public static readonly Error PaymentExceedsRemainingAmount = new Error(
            "Invoice.PaymentExceedsRemainingAmount",
            "The payment is greater than the partial amount remaining.");
    }
}
