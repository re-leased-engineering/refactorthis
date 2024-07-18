using System.ComponentModel.DataAnnotations.Schema;
using RefactorThis.Domain.Repositories.Interfaces;

namespace RefactorThis.Domain.Entities;

public class Payment : IEntity<int>
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    
    [ForeignKey("InvoiceId")]
    public virtual Invoice Invoice { get; set; }
}