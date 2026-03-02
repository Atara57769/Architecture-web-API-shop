using Entities;
using Moq;
using Moq.EntityFrameworkCore;
using Repositories;

namespace TestProject
{
    public class RatingRepositoryUnitTests
    {
        [Fact]
        public async Task AddRating_ReturnsRating_WhenCalled()
        {
            // Arrange
            var newRating = new Rating
            {
                Host = "localhost",
                Method = "POST",
                Path = "/api/User/login",
                RecordDate = DateTime.Now
            };

            var mockContext = new Mock<ApiDBContext>();

            mockContext.Setup(x => x.Ratings.AddAsync(It.IsAny<Rating>(), It.IsAny<CancellationToken>()))
                       .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Rating>>(null as Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Rating>));

            var repository = new RatingRepository(mockContext.Object);

            // Act
            var result = await repository.AddRating(newRating);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("POST", result.Method);
            Assert.Equal("localhost", result.Host);

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
