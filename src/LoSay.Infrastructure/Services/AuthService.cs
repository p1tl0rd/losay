using LoSay.Application.DTOs.Auth;
using LoSay.Application.Repositories;
using LoSay.Application.Services;

namespace LoSay.Infrastructure.Services
{
	public class AuthService : IAuthService
	{
		private readonly IAuthRepository _authRepository;
		private readonly IUserRepository _userRepository;
		public AuthService(IAuthRepository authRepository, IUserRepository userRepository)
		{
			_authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
		}
		public async Task<string> LoginAsync(UserAuthDto userAuthDto)
		{
			var login = await _authRepository.LoginAsync(userAuthDto.Code, userAuthDto.Password);
			if (!string.IsNullOrEmpty(login))
			{
				var userExist = await _userRepository.GetUserByCode(userAuthDto.Code);
				if (userExist == null)
				{
					var user = await _authRepository.GetUserByCode(userAuthDto.Code);
					await _userRepository.CreateUser(user);
				}
				return login;
			}
			else
			{
				return null;
			}
		}

		public Task<bool> UserExistsAsync(string email)
		{
			throw new NotImplementedException();
		}
	}
}
