using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Dto;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/product")]
    public class ProductAPIController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ResponseDto _responseDto;
        private readonly IMapper _mapper;

        public ProductAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _responseDto = new();
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            try
            {
                var products = await _db.Products.ToListAsync();
                
                _responseDto.Result = _mapper.Map<IEnumerable<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }

            return _responseDto;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ResponseDto> GetById(int id)
        {
            try
            {
                var product = await _db.Products.FirstAsync(x => x.ProductId == id);
                _responseDto.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
            }

            return _responseDto;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Create([FromBody] ProductDto productDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();

                _responseDto.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] ProductDto couponDto)
        {
            try
            {
                Product product = _mapper.Map<Product>(couponDto);
                _db.Products.Update(product);
                _db.SaveChanges();

                _responseDto.Result = _mapper.Map<ProductDto>(product);
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Delete(int id)
        {
            try
            {
                Product product = await _db.Products.FirstAsync(x => x.ProductId == id);
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }
    }
}
