using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;

        public ProductsController(IProductService productService, IDistributedCache cache, IConfiguration configuration)
        {
            _productService = productService;
            _cache = cache;
            _configuration = configuration;
        }
        // GET: api/<CategoriesController>
        [HttpGet]
        public async Task<ActionResult<PageResponseDTO<ProductDTO>>> Get( [FromQuery] int?[] categoryIds,string? description,int? maxPrice,int? minPrice,int position=1, int skip=8)
        {
            string categoryIdsStr = categoryIds != null ? string.Join(",", categoryIds.Where(c => c.HasValue).Select(c => c.Value)) : "";
            string cacheKey = $"products_{categoryIdsStr}_{description ?? ""}_{maxPrice ?? 0}_{minPrice ?? 0}_{position}_{skip}";
            string cachedResponse = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedResponse))
            {
                PageResponseDTO<ProductDTO> pageResponse = JsonSerializer.Deserialize<PageResponseDTO<ProductDTO>>(cachedResponse);
                return Ok(pageResponse);
            }
            PageResponseDTO<ProductDTO> pageResponse = await _productService.GetProducts(position,skip,categoryIds, description, maxPrice,minPrice);
            if (pageResponse.Data.Count() > 0)
            {
                string serializedResponse = JsonSerializer.Serialize(pageResponse);
                var ttlMinutes = _configuration.GetValue<int>("CacheSettings:ProductCacheTTLMinutes");
                await _cache.SetStringAsync(cacheKey, serializedResponse, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes)
                });
                return Ok(pageResponse);
            }
            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            string cacheKey = $"product_{id}";
            string cachedProduct = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProduct))
            {
                ProductDTO product = JsonSerializer.Deserialize<ProductDTO>(cachedProduct);
                return Ok(product);
            }
            ProductDTO product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();
            string serializedProduct = JsonSerializer.Serialize(product);
            var ttlMinutes = _configuration.GetValue<int>("CacheSettings:ProductCacheTTLMinutes");
            await _cache.SetStringAsync(cacheKey, serializedProduct, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ttlMinutes)
            });
            return Ok(product);
        }

    }
}
