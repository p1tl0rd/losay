using System.Security.Claims;
using LoSay.Application.DTOs;
using LoSay.Application.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace LoSay.Infrastructure.Services
{
	public class UserService : IUserService
	{
		private readonly AuthenticationStateProvider _authStateProvider;
		public UserService(AuthenticationStateProvider authStateProvider)
		{
			_authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
		}
		public async Task<string> GetUserLogin() 
		{
			var authState = await _authStateProvider.GetAuthenticationStateAsync();
			var user = authState.User;

			if (user?.Identity == null || !user.Identity.IsAuthenticated)
				return null;

			return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

		}
	 
	}
}
