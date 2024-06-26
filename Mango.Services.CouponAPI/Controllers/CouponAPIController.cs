﻿using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Dto;
using Mango.Services.CouponAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Controllers
{
    [Route("api/coupon")]
    [ApiController]
    [Authorize]
    public class CouponAPIController : ControllerBase
    {
        private readonly AppDbContext _db;    
        private readonly ResponseDto _responseDto;
        private readonly IMapper _mapper;

        public CouponAPIController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _responseDto = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ResponseDto> Get()
        {
            try
            {
                IEnumerable<Coupon> objList = await _db.Coupons.ToListAsync();
                _responseDto.Result = _mapper.Map<IEnumerable<CouponDto>>(objList);
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ResponseDto> Get(int id)
        {
            try
            {
                Coupon coupon = await _db.Coupons.FirstAsync(x => x.CouponId == id);
                _responseDto.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }

        [HttpGet]
        [Route("GetBycode/{code}")]
        public async Task<ResponseDto> GetByCode(string code)
        {
            try
            {
                Coupon coupon = await _db.Coupons.FirstAsync(x => x.CouponCode.ToLower() == code.ToLower());
                _responseDto.Result = _mapper.Map<CouponDto>(coupon);
            }
            catch (Exception e)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = e.Message;
            }

            return _responseDto;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ResponseDto> Post([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);
                await _db.Coupons.AddAsync(coupon);
                await _db.SaveChangesAsync();

                _responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
        public ResponseDto Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon coupon = _mapper.Map<Coupon>(couponDto);
                _db.Coupons.Update(coupon);
                _db.SaveChanges();

                _responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
        public ResponseDto Delete(int id)
        {
            try
            {
                Coupon coupon = _db.Coupons.First( x=> x.CouponId == id);
                _db.Coupons.Remove(coupon);
                _db.SaveChanges();
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
