using DAL;
using Microsoft.EntityFrameworkCore;

namespace BLL.Tests
{

    public static class TestDbContextFactory
    {

        public static DojoDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DojoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new DojoDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
