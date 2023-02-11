using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tweetbook.Contracts.V1.Responses;

namespace Tweetbook.Filters;
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        //before controller

        if (!context.ModelState.IsValid)
        {
            var errorsInModelState = context.ModelState
                .Where(a => a.Value.Errors.Count > 0)
                .ToDictionary(a => a.Key, a => a.Value.Errors.Select(a => a.ErrorMessage).ToArray());

            var errorResponse = new ErrorResponse();

            foreach (var error in errorsInModelState)
            {
                foreach (var subError in error.Value)
                {
                    var errorModel = new ErrorModel
                    {
                        FieldName = error.Key,
                        Message = subError
                    };

                    errorResponse.Errors.Add(errorModel);
                }
            }

            context.Result = new BadRequestObjectResult(errorResponse);
            return;
        }

        await next();

        //after controller
    }

}
