using System;

namespace RefactorThis.Persistence
{
	public class Payment
	{
		public decimal AmountPaid { get; set; }
		public string Reference { get; set; }
		public bool IsProcessed { get; private set; } = false;

		public void MarkAsProcessed() => IsProcessed = true;
	}
}