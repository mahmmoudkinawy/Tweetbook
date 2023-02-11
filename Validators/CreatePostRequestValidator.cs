using FluentValidation;
using Tweetbook.Contracts.V1.Requests;

namespace Tweetbook.Validators;
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9 ]*$");

        //RuleFor(x => x.Tags)
        //    .Must(); // Special validators
    }
}
