using Microsoft.EntityFrameworkCore;

namespace NonFactors.Mvc.Lookup.Tests.Objects.Data
{
    public class Context : DbContext
    {
        protected DbSet<TestModel> TestModel { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Mvc6LookupTest;Trusted_Connection=True;MultipleActiveResultSets=True");
        }
    }
}
