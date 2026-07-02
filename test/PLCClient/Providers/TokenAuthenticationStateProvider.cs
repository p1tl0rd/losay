using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace PLCClient.Providers
{
	public class TokenAuthenticationStateProvider : AuthenticationStateProvider
	{
		private readonly ProtectedSessionStorage _sessionStorage;

		public TokenAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
		{
			_sessionStorage = sessionStorage;
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			try
			{
				var result = await _sessionStorage.GetAsync<string>("token");

				if (!result.Success || string.IsNullOrWhiteSpace(result.Value))
				{
					return new AuthenticationState(
						new ClaimsPrincipal(new ClaimsIdentity())
					);
				}

				var claims = new JwtSecurityTokenHandler()
					.ReadJwtToken(result.Value)
					.Claims;

				var identity = new ClaimsIdentity(claims, "jwt");
				return new AuthenticationState(new ClaimsPrincipal(identity));
			}
			catch (InvalidOperationException)
			{
				// 👉 Đang prerender → JS interop chưa sẵn sàng
				return new AuthenticationState(
					new ClaimsPrincipal(new ClaimsIdentity())
				);
			}
		}
		public async Task Login(string token)
		{
			await _sessionStorage.SetAsync("token", token);
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}

		public async Task Logout()
		{
			await _sessionStorage.DeleteAsync("token");
			NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
		}
	}
}
