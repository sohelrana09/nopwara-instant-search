using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.NopwaraSearch.Models;

/// <summary>
/// Represents the plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.AllowedThemes")]
    public string AllowedThemes { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.ShowCategories")]
    public bool ShowCategories { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.MaxCategories")]
    public int MaxCategories { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.ShowManufacturers")]
    public bool ShowManufacturers { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.MaxManufacturers")]
    public int MaxManufacturers { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.ShowPrices")]
    public bool ShowPrices { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.ShowPopularTerms")]
    public bool ShowPopularTerms { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.NumberOfPopularTerms")]
    public int NumberOfPopularTerms { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.NopwaraSearch.Fields.DebounceMs")]
    public int DebounceMs { get; set; }
}
