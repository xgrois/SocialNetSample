using FluentValidation;
using SocialNet.Api.Models;

namespace SocialNet.Api.Validators;

public class UserInDtoValidator : AbstractValidator<UserInDto>
{
    public UserInDtoValidator()
    {
        RuleFor(u => u.Email).NotNull().NotEmpty();
    }
}
