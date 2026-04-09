using Microsoft.AspNetCore.Mvc;

namespace User.CustomAttribute
{
    public class ApiKeyAttribute: ServiceFilterAttribute
    {
        public ApiKeyAttribute()
            :base(typeof(ApiKeyAuthFilter))
        {
        }
    }
}
