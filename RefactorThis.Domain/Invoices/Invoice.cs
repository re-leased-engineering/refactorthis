using RefactorThis.Domain.Abstractions;
using RefactorThis.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Invoices
{
    public sealed class Invoice : Entity
    {
        private readonly List<Payment> _payments;

        private Invoice(
            Guid id,
            decimal amount,
            decimal amountPaid,
            decimal taxAmount,
            List<Payment> payments,
            InvoiceType type)
            : base(id)
        {
            Amount = amount;
            AmountPaid = amountPaid;
            TaxAmount = taxAmount;
            _payments = payments ?? new List<Payment>(); 
            Type = type;
        }

        public decimal Amount { get; private set; }
        public decimal AmountPaid { get; private set; }
        public decimal TaxAmount { get; private set; }
        public IReadOnlyList<Payment> Payments => _payments.AsReadOnly(); 
        public InvoiceType Type { get; private set; }

        public static Invoice Create(
            Guid id,
            decimal amount,
            decimal amountPaid,
            decimal taxAmount,
            List<Payment> payments,
            InvoiceType type)
        {
            return new Invoice(id, amount, amountPaid, taxAmount, payments, type);
        }

        public void AddPayment(Payment payment)
        {
            _payments.Add(payment);
            AmountPaid += payment.Amount;
            if (Type == InvoiceType.Commercial)
            {
                TaxAmount += payment.Amount * 0.14m;
            }
        }
        public bool IsPaymentExceedingRemainingAmount(decimal requestAmount)
        {
            return Payments.Sum(x => x.Amount) != 0 && requestAmount > (Amount - AmountPaid);
        }

        public bool IsAmountExceedingRequest(decimal requestAmount)
        {
            return requestAmount > Amount;
        }

        public bool IsFinalPayment(decimal paymentAmount)
        {
            return (Amount - AmountPaid) == paymentAmount;
        }

        public bool IsFullPayment(decimal paymentAmount)
        {
            return Amount == paymentAmount;
        }

        public bool IsAmountZero => Amount == 0;
        public bool HasNoPayments => _payments == null || !_payments.Any();
        public bool HasPayments => _payments != null && _payments.Any();
        public bool IsFullyPaid => Payments.Sum(x => x.Amount) != 0 && Amount == Payments.Sum(x => x.Amount);
    }

}
