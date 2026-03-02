using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly ApiDBContext _apiDbContext;
        public RatingRepository(ApiDBContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }
        public async Task<Rating> AddRating(Rating newRating)
        {
            await _apiDbContext.Ratings.AddAsync(newRating);
            await _apiDbContext.SaveChangesAsync();
            return newRating;
        }
    }
}
