using System;

namespace RefactorThis.Domain
{
	public class Payment
	{
		public decimal AmountPaid { get; set; }
		public string? Reference { get; set; }
		public PaymentStatus Status { get; private set; } = PaymentStatus.New;
		public string Remarks { get; set; }

		public void MarkAsPaid() => Status = PaymentStatus.Paid;
		public void MarkAsInitialised() => Status = PaymentStatus.Initialised;
		public void MarkAsDeclined() => Status = PaymentStatus.Declined;

	}

	public enum PaymentStatus
	{
		New,
		Initialised,
		Paid,
		Declined
	}
}