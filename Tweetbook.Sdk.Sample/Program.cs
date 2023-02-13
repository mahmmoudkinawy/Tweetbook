using Refit;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Sdk;

var cachedToken = string.Empty;

var identityApi = RestService.For<IIdentityApi>("http://localhost:5146");
var tweetbookApi = RestService.For<ITweetbookApi>("http://localhost:5146",
    new RefitSettings
    {
        AuthorizationHeaderValueGetter = () => Task.FromResult(cachedToken)
    });

var registerResponse = await identityApi.RegisterAsync(new UserRegisterationRequest
{
    Email = "sdkaccount@gmail.com",
    Password = "Pa$$w0rd"
});

var loginResponse = await identityApi.LoginAsync(new UserLoginRequest
{
    Email = "sdkaccount@gmail.com",
    Password = "Pa$$w0rd"
});

cachedToken = loginResponse.Content!.Token;

var allPosts = await tweetbookApi.GetAllAsync();

var createdPost = await tweetbookApi.CreateAsync(new CreatePostRequest
{
    Name = "post from sdk",
    Tags = new List<string>
    {
        "tag 1 from sdk",
        "tag 2 from sdk",
        "tag 3 from sdk",
    }
});

var retrivedPost = await tweetbookApi.GetAsync(createdPost.Content.Id);

var updatedPost = await tweetbookApi.UpdateAsync(createdPost.Content.Id, new UpdatePostRequest
{
    Name = "updated post from sdk"
});

var deletedPost = await tweetbookApi.DeleteAsync(createdPost.Content.Id);

Console.ReadKey();