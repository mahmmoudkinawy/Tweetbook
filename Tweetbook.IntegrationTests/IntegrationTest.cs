using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Data;

namespace Tweetbook.IntegrationTests;
public class IntegrationTest : IDisposable
{
    protected readonly HttpClient TestClient;
    private readonly IServiceProvider _serviceProvider;

    public IntegrationTest()
    {
        var appFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(DataContext));
                    services.AddDbContext<DataContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });
                });
            });
        _serviceProvider = appFactory.Services;
        TestClient = appFactory.CreateClient();
    }

    protected async Task AuthenticateAsync()
    {
        TestClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("bearer", await GetJwtAsync());
    }

    protected async Task<PostResponse> CreatePostAsync (CreatePostRequest request)
    {
        var response =  await TestClient.PostAsJsonAsync(ApiRoutes.Posts.Create, request);
        return await response.Content.ReadFromJsonAsync<PostResponse>();
    }

    private async Task<string> GetJwtAsync()
    {
        var response = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.Login, new UserRegisterationRequest
        {
            Email = "bob@test.com",
            Password = "Pa$$w0rd"
        });

        var content = await response.Content.ReadAsStringAsync();
        var registerationResponse = JsonSerializer.Deserialize<AuthSuccessResponse>(
            content, new JsonSerializerOptions
            {
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        return registerationResponse.Token;
    }

    public void Dispose()
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var context = serviceScope.ServiceProvider.GetService<DataContext>();
        context.Database.EnsureDeleted();
    }
}
