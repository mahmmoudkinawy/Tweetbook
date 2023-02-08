namespace Tweetbook.Extenstions;
public static class GeneralExtentions
{
    public static string GetUserId(this HttpContext httpContext)
    {
        if(httpContext.User == null)
        {
            return string.Empty;
        }

        return httpContext.User.Claims.Single(a => a.Type == "id").Value;
    }
}
