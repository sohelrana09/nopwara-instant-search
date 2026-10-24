using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.NopwaraSearch.Models;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Framework.Themes;

namespace Nop.Plugin.Widgets.NopwaraSearch.Components;

/// <summary>
/// Renders the instant search dropdown inside the header search form
/// </summary>
public class NopwaraSearchViewComponent : NopViewComponent
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly INopFileProvider _fileProvider;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly IThemeContext _themeContext;
    private readonly NopwaraSearchSettings _settings;

    #endregion

    #region Ctor

    public NopwaraSearchViewComponent(CatalogSettings catalogSettings,
        INopFileProvider fileProvider,
        INopUrlHelper nopUrlHelper,
        IThemeContext themeContext,
        NopwaraSearchSettings settings)
    {
        _catalogSettings = catalogSettings;
        _fileProvider = fileProvider;
        _nopUrlHelper = nopUrlHelper;
        _themeContext = themeContext;
        _settings = settings;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        //the markup is styled by the theme, so only render it for the allowed themes (others keep the core autocomplete)
        var theme = await _themeContext.GetWorkingThemeNameAsync();
        var allowed = (_settings.AllowedThemes ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains(theme, StringComparer.OrdinalIgnoreCase);
        if (!allowed)
            return Content(string.Empty);

        var model = new PublicInfoModel
        {
            SuggestUrl = _nopUrlHelper.RouteUrl(NopwaraSearchDefaults.SUGGEST_ROUTE_NAME),
            MinTermLength = Math.Max(1, _catalogSettings.ProductSearchTermMinimumLength),
            DebounceMs = Math.Max(0, _settings.DebounceMs),
            ShowImages = _catalogSettings.ShowProductImagesInSearchAutoComplete,
            //nopCommerce 4.90 keeps no per-customer search history
            ShowSearchHistory = false
        };

        //themes can override the view at Themes/{theme}/Views/Plugins/NopwaraSearch/PublicInfo.cshtml
        var themeView = $"~/Themes/{theme}/Views/Plugins/{NopwaraSearchDefaults.THEME_VIEWS_FOLDER}/PublicInfo.cshtml";
        var view = _fileProvider.FileExists(_fileProvider.MapPath(themeView))
            ? themeView
            : $"~/Plugins/{NopwaraSearchDefaults.PLUGIN_FOLDER}/Views/PublicInfo.cshtml";

        return View(view, model);
    }

    #endregion
}
