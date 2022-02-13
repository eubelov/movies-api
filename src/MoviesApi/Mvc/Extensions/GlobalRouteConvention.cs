using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

using MoviesApi.Mvc.Attributes;

namespace MoviesApi.Mvc.Extensions;

public class GlobalRouteConvention : IApplicationModelConvention
{
    private readonly AttributeRouteModel routePrefix;

    public GlobalRouteConvention(IRouteTemplateProvider route)
    {
        if (route is null)
        {
            throw new ArgumentNullException(nameof(route));
        }

        this.routePrefix = new(route);
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.Where(x => x.Attributes.Any(attr => attr is VersionedEndpointAttribute))
                     .SelectMany(c => c.Selectors))
        {
            selector.AttributeRouteModel = selector.AttributeRouteModel != null
                                               ? AttributeRouteModel.CombineAttributeRouteModel(
                                                   this.routePrefix,
                                                   selector.AttributeRouteModel)
                                               : this.routePrefix;
        }
    }
}