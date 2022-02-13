using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MoviesApi.Mvc.Extensions;

public static class MvcOptionsRouteExtensions
{
    public static void UseGlobalRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
    {
        if (routeAttribute is null)
        {
            throw new ArgumentNullException(nameof(routeAttribute));
        }

        opts.Conventions.Add(new GlobalRouteConvention(routeAttribute));
    }

    public static void UseGlobalRoutePrefix(this MvcOptions opts, string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentException($"{nameof(prefix)} cannot be empty", nameof(prefix));
        }

        opts.UseGlobalRoutePrefix(new RouteAttribute(prefix));
    }
}