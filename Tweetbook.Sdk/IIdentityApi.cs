using Refit;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;

namespace Tweetbook.Sdk;
public interface IIdentityApi
{
    [Post("/api/v1/identity/register")]
    Task<ApiResponse<AuthSuccessResponse>> RegisterAsync([Body] UserRegisterationRequest registerationRequest);

    [Post("/api/v1/identity/login")]
    Task<ApiResponse<AuthSuccessResponse>> LoginAsync([Body] UserLoginRequest loginRequest);

    [Post("/api/v1/identity/refresh")]
    Task<ApiResponse<AuthSuccessResponse>> RefreshAsync([Body] RefreshTokenRequest refreshToken);
}
