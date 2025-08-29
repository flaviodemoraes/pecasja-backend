using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PecasJa.Backend.Api.Domain;

namespace PecasJa.Backend.Api.Features.Authentication;

public static class Login
{
    public record Command(string Email, string Password) : IRequest<Result>;

    public class Result
    {
        public bool Succeeded { get; init; }
        public string Token { get; init; } = string.Empty;
        public string[] Errors { get; init; } = Array.Empty<string>();

        public static Result Success(string token) => new() { Succeeded = true, Token = token };
        public static Result Failure(string error) => new() { Succeeded = false, Errors = new[] { error } };
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public Handler(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Result.Failure("Invalid credentials.");
            }

            if (user.UserName is null || user.Email is null)
            {
                return Result.Failure("User data is incomplete.");
            }

            var authClaims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = GetToken(authClaims);

            return Result.Success(new JwtSecurityTokenHandler().WriteToken(token));
        }

        private JwtSecurityToken GetToken(IEnumerable<Claim> authClaims)
        {
            var secret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}
