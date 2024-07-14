using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Payments
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime PaymentDate { get; private set; }
        public string Reference { get; private set; }

        private Payment() { }

        public Payment(Guid id, decimal amount, string reference)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than zero.");
            }

            Id = id;
            Amount = amount;         
            Reference = reference;
        }
        public static Payment Create(string reference, decimal amount)
        {
            Guid id = Guid.NewGuid();
            return new Payment(id, amount, reference);
        }
        public void ValidateAmount(decimal invoiceAmount)
        {
            if (Amount > invoiceAmount)
            {
                throw new InvalidOperationException("The payment amount cannot exceed the invoice amount.");
            }
        }
    }
}
