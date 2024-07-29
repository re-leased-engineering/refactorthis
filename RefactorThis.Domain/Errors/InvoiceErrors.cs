namespace RefactorThis.Domain.Errors;

public static class InvoiceErrors
{
    public const string NoPaymentNeeded = "no payment needed";
    public const string InvoiceInvalidState = "The invoice is in an invalid state, it has an amount of 0 and it has payments.";
    public const string InvoiceAlreadyFullPaid = "invoice was already fully paid";
    public const string PaymentIsGreaterthanPartialAmountRemaining = "the payment is greater than the partial amount remaining";
    public const string PaymentIsGreaterthanInvoiceAmount = "the payment is greater than the invoice amount";
    public const string NoInvoiceFound = "There is no invoice matching this payment";
}
