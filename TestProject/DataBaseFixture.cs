using Castle.Core.Configuration;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class DatabaseFixture : IDisposable
    {
        public ApiDBContext Context { get; private set; }
        private readonly string _dbName;
        public DatabaseFixture()
        {
            _dbName = $"ApiDB_test_{Guid.NewGuid()}";
            var connectionString = $"Data Source=ATARA; Initial Catalog={_dbName}; Integrated Security=True; Trust Server Certificate=True";

            var options = new DbContextOptionsBuilder<ApiDBContext>()
                .UseSqlServer(connectionString)
                .Options;

            Context = new ApiDBContext(options);
            Context.Database.EnsureCreated();
        }
        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
