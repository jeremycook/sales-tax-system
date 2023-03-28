using Microsoft.EntityFrameworkCore;

namespace SiteKit.EntityFrameworkCore
{
    /// <summary>
    /// Implementations can contribute to an <see cref="ModelBuilder"/>.
    /// </summary>
    public interface IModelBuilderContributor
    {
        /// <summary>
        /// Contribute modifications to the <paramref name="modelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public void ContributeTo(ModelBuilder modelBuilder);
    }
}
