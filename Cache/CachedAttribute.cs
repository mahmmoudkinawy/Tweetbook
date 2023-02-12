using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Tweetbook.Services;

namespace Tweetbook.Cache;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CachedAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveSeconds;

    public CachedAttribute(int timeToLiveSeconds)
    {
        _timeToLiveSeconds = timeToLiveSeconds;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        //before
        var cacheSettings = context.HttpContext.RequestServices
            .GetRequiredService<RedisCacheSettings>();

        if (!cacheSettings.Enabled)
        {
            await next();
            return;
        }

        var cacheService = context.HttpContext.RequestServices
            .GetRequiredService<IResponseCacheService>();

        var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
        var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedResponse))
        {
            var contentResult = new ContentResult
            {
                Content = cachedResponse,
                ContentType = "application/json",
                StatusCode = 200
            };

            context.Result = contentResult;
            return;
        }

        var executedContext = await next();

        if(executedContext.Result is OkObjectResult okObjectResult)
        {
            await cacheService.CacheResponseAsync(
                cacheKey,
                okObjectResult.Value,
                TimeSpan.FromSeconds(_timeToLiveSeconds));
        }

        //after
    }

    private static string GenerateCacheKeyFromRequest(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();

        keyBuilder.Append(request.Path);

        foreach (var (key, value) in request.Query.OrderBy(a => a.Key))
        {
            keyBuilder.Append($"|{key}-{value}");
        }

        return keyBuilder.ToString();
    }

}
