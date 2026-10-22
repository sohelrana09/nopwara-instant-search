using Nop.Core.Caching;

namespace Nop.Plugin.Widgets.NopwaraSearch;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class NopwaraSearchDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public const string SYSTEM_NAME = "Widgets.NopwaraSearch";

    /// <summary>
    /// Gets the folder (under Nop.Web/Plugins) the plugin is deployed to
    /// </summary>
    public const string PLUGIN_FOLDER = "Widgets.NopwaraSearch";

    /// <summary>
    /// Gets the folder name themes use to override the plugin views (Themes/{theme}/Views/Plugins/{folder})
    /// </summary>
    public const string THEME_VIEWS_FOLDER = "NopwaraSearch";

    /// <summary>
    /// Gets the name of the suggestions (ajax) route
    /// </summary>
    public const string SUGGEST_ROUTE_NAME = "Plugin.Widgets.NopwaraSearch.Suggest";

    /// <summary>
    /// Gets the name of the admin configuration route
    /// </summary>
    public const string CONFIGURATION_ROUTE_NAME = "Plugin.Widgets.NopwaraSearch.Configure";

    /// <summary>
    /// Gets the key for caching the popular search terms
    /// </summary>
    /// <remarks>
    /// {0} : store identifier
    /// {1} : number of terms
    /// </remarks>
    public static CacheKey PopularTermsCacheKey => new("Nop.plugins.widgets.nopwarasearch.popularterms.{0}-{1}");
}
