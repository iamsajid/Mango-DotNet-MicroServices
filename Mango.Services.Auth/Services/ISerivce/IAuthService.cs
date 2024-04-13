using Mango.Services.AuthAPI.Dto;

namespace Mango.Services.AuthAPI.Services.ISerivce
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegistrationRequestDto registrationRequestDto);

        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto);
    }
}
