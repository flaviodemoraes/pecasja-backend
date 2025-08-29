using MediatR;
using Microsoft.AspNetCore.Identity;
using PecasJa.Backend.Api.Domain;

namespace PecasJa.Backend.Api.Features.Authentication;

public static class CreateUser
{
    public record Command(string Email, string Password) : IRequest<Result>;

    public class Result
    {
        public bool Succeeded { get; init; }
        public string[] Errors { get; init; } = Array.Empty<string>();

        public static Result Success() => new() { Succeeded = true };
        public static Result Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors.ToArray() };
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var identityResult = await _userManager.CreateAsync(newUser, request.Password);

            return identityResult.Succeeded
                ? Result.Success()
                : Result.Failure(identityResult.Errors.Select(e => e.Description));
        }
    }
}
