using Swashbuckle.AspNetCore.Filters;
using Tweetbook.Contracts.V1.Responses;

namespace Tweetbook.SwaggerExamples.Responses;
public class PostResponseExample : IExamplesProvider<PostResponse>
{
    public PostResponse GetExamples()
    {
        return new PostResponse
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Tags = new List<TagResponse>
            {
                new TagResponse{Name = "new tag"},
                new TagResponse{Name = "new tag 2"},
            },
            UserId = Guid.NewGuid().ToString()
        };
    }
}
