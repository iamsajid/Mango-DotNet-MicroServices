using AutoMapper;
using Mango.Services.CouponAPI.Dto;
using Mango.Services.CouponAPI.Models;

namespace Mango.Services.CouponAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingconfig = new MapperConfiguration(config =>
            {
                config.CreateMap<CouponDto, Coupon>();
                config.CreateMap<Coupon, CouponDto>();
            });

            return mappingconfig;
        }
    }
}
