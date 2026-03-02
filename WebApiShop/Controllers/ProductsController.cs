using DTOs;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        // GET: api/<CategoriesController>
        [HttpGet]
        public async Task<ActionResult<PageResponseDTO<ProductDTO>>> Get( [FromQuery] int?[] categoryIds,string? description,int? maxPrice,int? minPrice,int position=1, int skip=8)
        {
            PageResponseDTO<ProductDTO> pageResponse = await _productService.GetProducts(position,skip,categoryIds, description, maxPrice,minPrice);
            if (pageResponse.Data.Count() > 0)
                return Ok(pageResponse);
            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> Get(int id)
        {
            ProductDTO product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

    }
}
