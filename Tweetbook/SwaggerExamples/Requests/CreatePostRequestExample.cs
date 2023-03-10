using Swashbuckle.AspNetCore.Filters;
using Tweetbook.Contracts.V1.Requests;

namespace Tweetbook.SwaggerExamples.Requests;
public class CreatePostRequestExample : IExamplesProvider<CreatePostRequest>
{
    public CreatePostRequest GetExamples()
    {
        return new CreatePostRequest
        {
            Name = "new name",
            Tags = new List<string>
            {
                 "some tag",
                 "some tag"
            }
        };
    }
}
