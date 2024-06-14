using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Dto;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private ResponseDto _response;
        private IMapper _mapper;
        private readonly AppDbContext _db;

        public CartAPIController(IMapper mapper, AppDbContext db)
        {
            _response = new();
            _mapper = mapper;
            _db = db;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                CartDto cart = new CartDto
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeaders.First(x => x.UserId == userId))
                };
                cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails
                    .Where(x => x.CartHeaderId == cart.CartHeader.CartHeaderId));

                foreach (var item in cart.CartDetails)
                {
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                _response.Result = cart;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserId == cartDto.CartHeader.UserId);

                if (cartHeaderFromDb == null)
                {
                    // User add first item to the cart
                    var cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;

                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // user updates an exisitng cart
                    // if cart header is null
                    // check if details has same product or not
                    var cartDetailsFromDb = await _db.CartDetails.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ProductId == cartDto.CartDetails.First().ProductId 
                                            && x.CartHeaderId == cartDto.CartHeader.CartHeaderId);

                    if (cartDetailsFromDb == null)
                    {
                        // create cart details
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {

                        // user updates quantity of existing items in the cart
                        // update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;

                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                }

                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody] int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails = await _db.CartDetails.FirstAsync(x => x.CartDetailsId == cartDetailsId);

                int totalCountofCartItem = _db.CartDetails.Where(x => x.CartHeaderId == cartDetails.CartHeaderId).Count();

                _db.CartDetails.Remove(cartDetails);

                if (totalCountofCartItem == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(x => x.CartHeaderId == cartDetails.CartHeaderId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }
    }
}
