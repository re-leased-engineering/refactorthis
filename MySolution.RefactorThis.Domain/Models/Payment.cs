namespace MySolution.RefactorThis.Domain.Models
{
    public class Payment
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
