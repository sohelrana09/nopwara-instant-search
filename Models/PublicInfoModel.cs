using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.NopwaraSearch.Models;

/// <summary>
/// Represents the settings the instant search dropdown is rendered with
/// </summary>
public record PublicInfoModel : BaseNopModel
{
    public string SuggestUrl { get; set; }

    public string DeleteTermUrl { get; set; }

    public int MinTermLength { get; set; }

    public int DebounceMs { get; set; }

    public bool ShowImages { get; set; }

    public bool ShowSearchHistory { get; set; }
}
