using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    public class Invoice
    {
        private readonly decimal _taxPercentage = 0.14m;
        private readonly InvoiceRepository _repository;
        public Invoice(InvoiceRepository repository)
        {
            _repository = repository;
        }

        public void Save()
        {
            _repository.SaveInvoice(this);
        }

        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }

        public InvoiceType Type { get; set; }
        public bool InvoiceHasPayments { get => Payments != null && Payments.Any(); }
        public bool InvoiceIsAlreadyFullyPaid { get => Payments.Sum(x => x.Amount) != 0 && Amount == Payments.Sum(x => x.Amount); }
        public bool IsPaymentsGreaterThanThePartial(Payment payment) => Payments.Sum(x => x.Amount) != 0 && payment.Amount > (Amount - AmountPaid);
        public string PartialPay(Payment payment)
        {
            if (!Type.IsInvoiceTypeSupported())
            {
                throw new ArgumentOutOfRangeException();
            }
            bool isPaymentFinalPartial = (Amount - AmountPaid) == payment.Amount;

            string responseMessage = isPaymentFinalPartial ? "final partial payment received, invoice is now fully paid" : "another partial payment received, still not fully paid";

            if (Type == InvoiceType.Commercial)
            {
                TaxAmount += payment.Amount * _taxPercentage;
            }

            AmountPaid += payment.Amount;
            Payments.Add(payment);

            return responseMessage;
        }

        public string FullPay(Payment payment)
        {
            if (!Type.IsInvoiceTypeSupported())
            {
                throw new ArgumentOutOfRangeException();
            }

            bool isFullPayment = Amount == payment.Amount;
            string responseMessage = isFullPayment ? "invoice is now fully paid" : "invoice is now partially paid";

            AmountPaid = payment.Amount;
            TaxAmount = payment.Amount * _taxPercentage;
            Payments.Add(payment);

            return responseMessage;
        }
    }

    public enum InvoiceType
    {
        Standard,
        Commercial
    }

    public static class InvoiceTypeExtension
    {
        public static bool IsInvoiceTypeSupported(this InvoiceType invoiceType)
        {
            return invoiceType == InvoiceType.Standard || invoiceType == InvoiceType.Commercial;
        }
    }

}