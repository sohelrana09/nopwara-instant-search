using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.NopwaraSearch.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.NopwaraSearch.Controllers;

[Area(AreaNames.ADMIN)]
[AuthorizeAdmin]
[AutoValidateAntiforgeryToken]
public class NopwaraSearchAdminController : BasePluginController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly NopwaraSearchSettings _settings;

    #endregion

    #region Ctor

    public NopwaraSearchAdminController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStaticCacheManager staticCacheManager,
        NopwaraSearchSettings settings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _staticCacheManager = staticCacheManager;
        _settings = settings;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public IActionResult Configure()
    {
        var model = new ConfigurationModel
        {
            AllowedThemes = _settings.AllowedThemes,
            ShowCategories = _settings.ShowCategories,
            MaxCategories = _settings.MaxCategories,
            ShowManufacturers = _settings.ShowManufacturers,
            MaxManufacturers = _settings.MaxManufacturers,
            ShowPrices = _settings.ShowPrices,
            ShowPopularTerms = _settings.ShowPopularTerms,
            NumberOfPopularTerms = _settings.NumberOfPopularTerms,
            DebounceMs = _settings.DebounceMs
        };

        return View($"~/Plugins/{NopwaraSearchDefaults.PLUGIN_FOLDER}/Views/Admin/Configure.cshtml", model);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return Configure();

        _settings.AllowedThemes = model.AllowedThemes?.Trim();
        _settings.ShowCategories = model.ShowCategories;
        _settings.MaxCategories = Math.Max(0, model.MaxCategories);
        _settings.ShowManufacturers = model.ShowManufacturers;
        _settings.MaxManufacturers = Math.Max(0, model.MaxManufacturers);
        _settings.ShowPrices = model.ShowPrices;
        _settings.ShowPopularTerms = model.ShowPopularTerms;
        _settings.NumberOfPopularTerms = Math.Max(0, model.NumberOfPopularTerms);
        _settings.DebounceMs = Math.Max(0, model.DebounceMs);

        await _settingService.SaveSettingAsync(_settings);
        await _staticCacheManager.RemoveByPrefixAsync("Nop.plugins.widgets.nopwarasearch");

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return Configure();
    }

    #endregion
}
