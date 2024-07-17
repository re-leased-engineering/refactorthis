using RefactorThis.Domain.Common;

namespace RefactorThis.Domain.Entities
{
    public class Payment : Entity
    {
        public decimal Amount { get; set; }
        public string Reference { get; set; }
    }
}