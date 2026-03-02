using DTOs;
using Entities;

namespace Services
{
    public interface IProductService
    {
        Task<PageResponseDTO<ProductDTO>> GetProducts(int position, int skip, int?[] categoryIds,
            string? description, int? maxPrice, int? minPrice);
        public Task<ProductDTO> GetProductById(int id);
        
    }
}