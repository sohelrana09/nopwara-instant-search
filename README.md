# Nopwara Instant Search for nopCommerce

An instant search dropdown for the nopCommerce header search box, built with **Tailwind CSS** and **Alpine.js** for the [Nopwara](../../Presentation/Nop.Web/Themes/Nopwara) theme. It replaces the core jQuery UI autocomplete.

## Features

- **Products as you type:** picture, name with the search term highlighted, price and old price.
- **Matching categories** (with their parent path) and **brands**.
- **Recent and popular searches** when the search box is focused while empty. Recent searches can be removed one at a time.
- **"See all N results"** link to the search page, which respects the search box category select.
- **Keyboard and screen readers:** ↑/↓ moves through results, Enter opens one, Esc closes the panel. The box uses ARIA combobox/listbox semantics.
- **Light on the browser:** requests are debounced, stale requests are cancelled, and results are cached in the browser. There is no jQuery in the dropdown.
- **Uses core search:** results come from core `IProductService.SearchProductsAsync`, so the active search provider (for example the Lucene plugin), store mapping, ACL and published state are all honoured.

## Requirements

- nopCommerce **5.00**
- A theme that styles the markup with Tailwind (Nopwara). Other themes keep the core autocomplete; see `AllowedThemes` below.

## Install

1. Copy this folder to `src/Plugins/` and add the project to `NopCommerce.sln`.
2. Build the solution. To build only the plugin:
   `dotnet build Plugins/Nop.Plugin.Widgets.NopwaraSearch/Nop.Plugin.Widgets.NopwaraSearch.csproj -p:SolutionDir=$PWD/` (run from `src`).
3. In admin, go to **Configuration → Local plugins**, install **Nopwara instant search**, then restart the application.
4. The widget is activated on install. You can turn it off again under **Configuration → Widgets**.

To theme the dropdown with Tailwind, the theme build has to scan the plugin views. Add these lines to the theme's Tailwind entry CSS, then rebuild the CSS:

```css
@source "../../../../../Plugins/Nop.Plugin.Widgets.NopwaraSearch/Views";
@source "../../../../../Plugins/Nop.Plugin.Widgets.NopwaraSearch/Content/js";
```

## Settings

**From the catalog settings** (Configuration → Settings → Catalog settings → Search):
- number of products
- minimum search term length
- product images
- search term history

**Plugin settings:**

| Setting | Default | |
|---|---|---|
| Themes (`AllowedThemes`) | `Nopwara` | Comma-separated theme system names the dropdown renders for |
| Show prices | on | |
| Show categories / Number of categories | on / 3 | Name match, published, store mapping and ACL applied |
| Show manufacturers / Number of manufacturers | on / 3 | |
| Show popular searches / Number | on / 6 | Most searched terms (cached for 1 hour); shown when the box is empty or nothing is found |
| Typing delay (ms) | 250 | |

## How it works

- **Widget:** the plugin renders a widget in the `searchbox` zone, inside the header search form. Its Alpine component attaches to the existing `.search-box-text` input and the `.search-box-category` select.
- **Endpoint:** `GET /nopwara-search/suggest?q=&cid=` returns JSON. With an empty `q` it returns recent and popular searches.
- **Turning off core autocomplete:** the theme's `SearchBox` view skips the core autocomplete script while the widget is active:

  ```razor
  var instantSearchActive = await widgetPluginManager.IsPluginActiveAsync("Widgets.NopwaraSearch", customer, storeId);
  @if (Model.AutoCompleteEnabled && !instantSearchActive) { ...core autocomplete script... }
  ```

- **Overriding the markup:** copy `Views/PublicInfo.cshtml` to `Themes/{YourTheme}/Views/Plugins/NopwaraSearch/PublicInfo.cshtml`.

## License

Copyright (C) 2026 Sohel Rana

Licensed under the [GNU Affero General Public License v3.0](LICENSE), the license nopCommerce itself is built on (the [nopCommerce Public License](https://www.nopcommerce.com/license) is AGPL v3 plus its additional terms). Parts of this project are adapted from nopCommerce source code. A store that uses it must keep nopCommerce's "Powered by nopCommerce" link, as the nopCommerce Public License requires.
