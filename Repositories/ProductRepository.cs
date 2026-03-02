using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApiDBContext _apiDbContext;
        public ProductRepository(ApiDBContext apiDbContext)
        {
            _apiDbContext = apiDbContext;
        }
        public async Task<(List<Product> Items, int TotalCount)> GetProducts(int position, int skip, int?[] categoryIds,
            string? description,int?maxPrice,int?minPrice)
        {
            var query = _apiDbContext.Products.Where(product =>
                (description == null ? (true) : (product.Description.Contains(description))) &&
                ((maxPrice == null) ? (true) : (product.Price <= maxPrice)) &&
                ((minPrice == null) ? (true) : (product.Price >= minPrice)) &&
                ((categoryIds.Length == 0) ? (true) : (categoryIds.Contains(product.CategoryId)))).OrderBy(product => product.Price);
            List<Product> products = await query.Skip((position-1)*skip).Take(skip).Include(product=>product.Category).ToListAsync();    
            var total =await query.CountAsync();
            return (products, total);
        }
        public async Task<Product> GetProductById(int id)
        {
            return await _apiDbContext.Products.FindAsync(id);
        }
    }
}
