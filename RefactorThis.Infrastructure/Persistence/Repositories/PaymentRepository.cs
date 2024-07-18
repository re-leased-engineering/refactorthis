using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Repositories.Interfaces;

namespace RefactorThis.Infrastructure.Persistence.Repositories;

public class PaymentRepository : BaseRepository<Payment, int>, IPaymentRepository
{
    
}