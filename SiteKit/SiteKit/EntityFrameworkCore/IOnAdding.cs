using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SiteKit.EntityFrameworkCore
{
    public interface IOnAdding
    {
        Task OnAdding<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;
    }
}
