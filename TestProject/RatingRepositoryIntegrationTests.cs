using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class RatingRepositoryIntegrationTests : IDisposable
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApiDBContext _dbContext;
        private readonly RatingRepository _ratingRepository;

        public RatingRepositoryIntegrationTests()
        {
            _fixture = new DatabaseFixture();
            _dbContext = _fixture.Context;
            _ratingRepository = new RatingRepository(_dbContext);
        }
        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public async Task AddRating_Integration_SavesToDatabase()
        {
            // Arrange
            var rating = new Rating
            {
                Host = "127.0.0.1",
                Method = "GET",
                Path = "/api/Products",
                UserAgent = "Mozilla/5.0"
            };

            // Act
            var result = await _ratingRepository.AddRating(rating);

            // Assert
            Assert.NotEqual(0, result.RatingId);

            _dbContext.ChangeTracker.Clear();
            var saved = await _dbContext.Ratings.FindAsync(result.RatingId);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal("GET", saved.Method.Trim()); 
            Assert.Equal("127.0.0.1", saved.Host.Trim());
        }

    }
}