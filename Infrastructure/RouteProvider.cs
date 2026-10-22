using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.NopwaraSearch.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(name: NopwaraSearchDefaults.CONFIGURATION_ROUTE_NAME,
            pattern: "Admin/NopwaraSearch/Configure",
            defaults: new { controller = "NopwaraSearchAdmin", action = "Configure", area = AreaNames.ADMIN });

        //no language prefix, like the core autocomplete endpoints (the working language comes from the customer)
        endpointRouteBuilder.MapControllerRoute(name: NopwaraSearchDefaults.SUGGEST_ROUTE_NAME,
            pattern: "nopwara-search/suggest",
            defaults: new { controller = "NopwaraSearch", action = "Suggest" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
