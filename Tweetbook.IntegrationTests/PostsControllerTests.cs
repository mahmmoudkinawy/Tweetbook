using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Domain;

namespace Tweetbook.IntegrationTests;
public class PostsControllerTests : IntegrationTest
{

    [Fact]
    public async Task GetAll_WithoutAnyPosts_ReturnsEmptyResponse()
    {
        //Arrange

        await AuthenticateAsync();

        //Act
        var response = await TestClient.GetAsync(ApiRoutes.Posts.GetAll);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<List<Post>>()).Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsPost_WhenPostExistsInTheDatabase()
    {
        await AuthenticateAsync();
        var createdPost = await CreatePostAsync(new CreatePostRequest
        {
            Name = "Test post"
        });

        var response = await TestClient
            .GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", createdPost.Id.ToString()));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returedPost = await response.Content.ReadFromJsonAsync<Post>();
        returedPost.Id.Should().Be(createdPost.Id);
        returedPost.Name.Should().Be("Test post");
    }

}
