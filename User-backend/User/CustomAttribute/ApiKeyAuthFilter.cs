using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using User.Interface;

namespace User.CustomAttribute
{
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private readonly IApiKeyValidation _apiKeyValidation;
        
        public ApiKeyAuthFilter(IApiKeyValidation apiKeyValidatin)
        {
            _apiKeyValidation = apiKeyValidatin;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var UserApiKey = context.HttpContext.Request.Headers[Constants.ApiKeyHeaderName];

            if (string.IsNullOrEmpty(UserApiKey))
            {
                context.Result = new BadRequestObjectResult("API Key missing or invalid");
                return;
            }

            if (! _apiKeyValidation.IsValidApiKey(UserApiKey)) 
            {
                context.Result = new UnauthorizedObjectResult("You are unauthorized");
                return;
            }
        }
    }
}
