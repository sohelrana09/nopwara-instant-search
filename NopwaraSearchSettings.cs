using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.NopwaraSearch;

/// <summary>
/// Represents the plugin settings
/// </summary>
/// <remarks>
/// The number of products, the minimum term length, product images and search history
/// come from the core catalog settings (Configuration > Settings > Catalog > Search).
/// </remarks>
public class NopwaraSearchSettings : ISettings
{
    /// <summary>
    /// Gets or sets comma-separated theme system names the dropdown is rendered for (its markup is styled by the theme)
    /// </summary>
    public string AllowedThemes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show matching categories
    /// </summary>
    public bool ShowCategories { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of categories to show
    /// </summary>
    public int MaxCategories { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show matching manufacturers
    /// </summary>
    public bool ShowManufacturers { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of manufacturers to show
    /// </summary>
    public int MaxManufacturers { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show product prices
    /// </summary>
    public bool ShowPrices { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show popular search terms when the search box is empty
    /// </summary>
    public bool ShowPopularTerms { get; set; }

    /// <summary>
    /// Gets or sets the number of popular search terms to show
    /// </summary>
    public int NumberOfPopularTerms { get; set; }

    /// <summary>
    /// Gets or sets the delay (milliseconds) after the last key press before searching
    /// </summary>
    public int DebounceMs { get; set; }
}
