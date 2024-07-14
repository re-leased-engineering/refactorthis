using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
