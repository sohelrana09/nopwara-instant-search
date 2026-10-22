using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.NopwaraSearch.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.NopwaraSearch;

/// <summary>
/// Represents the Nopwara instant search plugin
/// </summary>
public class NopwaraSearchPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public NopwaraSearchPlugin(ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        //"searchbox" is rendered inside the header search form
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.SearchBox });
    }

    /// <summary>
    /// Gets a type of a view component for displaying widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(NopwaraSearchViewComponent);
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(NopwaraSearchDefaults.CONFIGURATION_ROUTE_NAME);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new NopwaraSearchSettings
        {
            AllowedThemes = "Nopwara",
            ShowCategories = true,
            MaxCategories = 3,
            ShowManufacturers = true,
            MaxManufacturers = 3,
            ShowPrices = true,
            ShowPopularTerms = true,
            NumberOfPopularTerms = 6,
            DebounceMs = 250
        });

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(NopwaraSearchDefaults.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(NopwaraSearchDefaults.SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            //admin
            ["Plugins.Widgets.NopwaraSearch.Instructions"] = "Replaces the search box autocomplete with an instant search dropdown. The number of products, the minimum search term length, product images and the search history are taken from Configuration > Settings > Catalog settings > Search. Activate or deactivate the dropdown in Configuration > Widgets.",
            ["Plugins.Widgets.NopwaraSearch.Fields.AllowedThemes"] = "Themes",
            ["Plugins.Widgets.NopwaraSearch.Fields.AllowedThemes.Hint"] = "Comma-separated theme system names the dropdown is used with (its markup is styled by the theme). Other themes keep the core autocomplete.",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowCategories"] = "Show categories",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowCategories.Hint"] = "Show categories whose name matches the search term.",
            ["Plugins.Widgets.NopwaraSearch.Fields.MaxCategories"] = "Number of categories",
            ["Plugins.Widgets.NopwaraSearch.Fields.MaxCategories.Hint"] = "The maximum number of categories to show.",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowManufacturers"] = "Show manufacturers",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowManufacturers.Hint"] = "Show manufacturers whose name matches the search term.",
            ["Plugins.Widgets.NopwaraSearch.Fields.MaxManufacturers"] = "Number of manufacturers",
            ["Plugins.Widgets.NopwaraSearch.Fields.MaxManufacturers.Hint"] = "The maximum number of manufacturers to show.",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowPrices"] = "Show prices",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowPrices.Hint"] = "Show product prices in the dropdown.",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowPopularTerms"] = "Show popular searches",
            ["Plugins.Widgets.NopwaraSearch.Fields.ShowPopularTerms.Hint"] = "Show the most searched terms when the search box is empty or nothing is found.",
            ["Plugins.Widgets.NopwaraSearch.Fields.NumberOfPopularTerms"] = "Number of popular searches",
            ["Plugins.Widgets.NopwaraSearch.Fields.NumberOfPopularTerms.Hint"] = "The number of popular search terms to show.",
            ["Plugins.Widgets.NopwaraSearch.Fields.DebounceMs"] = "Typing delay (ms)",
            ["Plugins.Widgets.NopwaraSearch.Fields.DebounceMs.Hint"] = "How long to wait after the last key press before searching.",

            //storefront
            ["Plugins.Widgets.NopwaraSearch.Products"] = "Products",
            ["Plugins.Widgets.NopwaraSearch.Categories"] = "Categories",
            ["Plugins.Widgets.NopwaraSearch.Manufacturers"] = "Brands",
            ["Plugins.Widgets.NopwaraSearch.Recent"] = "Recent searches",
            ["Plugins.Widgets.NopwaraSearch.Popular"] = "Popular searches",
            ["Plugins.Widgets.NopwaraSearch.SeeAll"] = "See all {0} results",
            ["Plugins.Widgets.NopwaraSearch.NoResults"] = "No results for \"{0}\"",
            ["Plugins.Widgets.NopwaraSearch.Loading"] = "Searching...",
            ["Plugins.Widgets.NopwaraSearch.Remove"] = "Remove"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(NopwaraSearchDefaults.SYSTEM_NAME))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(NopwaraSearchDefaults.SYSTEM_NAME);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _settingService.DeleteSettingAsync<NopwaraSearchSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.NopwaraSearch");

        await base.UninstallAsync();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}
