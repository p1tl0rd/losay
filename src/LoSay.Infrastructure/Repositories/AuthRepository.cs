using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LoSay.Application.Repositories;
using LoSay.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LoSay.Infrastructure.Repositories
{
	public class AuthRepository : IAuthRepository
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IConfiguration _configuration;
		private readonly IUserRepository _userRepository;
		public AuthRepository(UserManager<User> userManager, SignInManager<User> signInManager,
			IConfiguration configuration, IUserRepository userRepository)
		{
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
		}

		public async Task<User> GetUserByCode(string code)
			=> await _userManager.FindByNameAsync(code);

		public async Task<string> LoginAsync(string code, string password)
		{
			var user = await _userManager.FindByNameAsync(code);
			if (user == null) return null;
			var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
			if (!isValidPassword) return null;

			var roles = await _userManager.GetRolesAsync(user);
			return CreateToken(user, roles);
		}

		public Task<int> RegisterAsync(User user, string password, int startUnitId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> UserExistsAsync(string email)
		{
			throw new NotImplementedException();
		}
		private string CreateToken(User user, IList<string>? roles)
		{

			IList<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim("LastName", user.FullName),
				new Claim("Code", user.Code),
				new Claim("Email", user.Email),
			};
			// Thêm vai trò vào claim
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}
			string symmetricKey = _configuration.GetSection("TokenSettings:Token").Value;
			var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(symmetricKey));
			var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				claims: claims,
				expires: DateTime.Now.AddDays(100),
				signingCredentials: creds);

			var jwt = new JwtSecurityTokenHandler().WriteToken(token);

			return jwt;
		}

	}
}
