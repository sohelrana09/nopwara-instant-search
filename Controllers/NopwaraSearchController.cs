using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Core.Http;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.NopwaraSearch.Controllers;

/// <summary>
/// Storefront endpoint of the instant search dropdown
/// </summary>
public class NopwaraSearchController : BasePublicController
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly ICategoryService _categoryService;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerService _manufacturerService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductService _productService;
    private readonly IRepository<SearchTerm> _searchTermRepository;
    private readonly ISearchTermService _searchTermService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly MediaSettings _mediaSettings;
    private readonly NopwaraSearchSettings _settings;

    #endregion

    #region Ctor

    public NopwaraSearchController(CatalogSettings catalogSettings,
        ICategoryService categoryService,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        INopUrlHelper nopUrlHelper,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IRepository<SearchTerm> searchTermRepository,
        ISearchTermService searchTermService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        NopwaraSearchSettings settings)
    {
        _catalogSettings = catalogSettings;
        _categoryService = categoryService;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _nopUrlHelper = nopUrlHelper;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _searchTermRepository = searchTermRepository;
        _searchTermService = searchTermService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _workContext = workContext;
        _mediaSettings = mediaSettings;
        _settings = settings;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Gets the most searched terms of a store (cached; the query groups the whole SearchTerm table)
    /// </summary>
    private async Task<IList<string>> GetPopularTermsAsync(int storeId)
    {
        if (!_settings.ShowPopularTerms || _settings.NumberOfPopularTerms <= 0)
            return [];

        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopwaraSearchDefaults.PopularTermsCacheKey, storeId, _settings.NumberOfPopularTerms);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            //core ISearchTermService.GetStatsAsync counts every store together
            var query = _searchTermRepository.Table
                .Where(term => term.StoreId == storeId && !term.Deleted)
                .GroupBy(term => term.Keyword)
                .Select(group => new { Keyword = group.Key, Count = group.Count() })
                .OrderByDescending(line => line.Count)
                .Select(line => line.Keyword);

            return (IList<string>)(await query.ToPagedListAsync(0, _settings.NumberOfPopularTerms)).ToList();
        });
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets suggestions for the search term; an empty term returns the recent and popular searches
    /// </summary>
    /// <param name="q">Search term</param>
    /// <param name="cid">Category identifier from the search box category select (0 = all)</param>
    [CheckLanguageSeoCode(ignore: true)]
    public virtual async Task<IActionResult> Suggest(string q, int cid = 0)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var term = q?.Trim() ?? string.Empty;
        var searchUrl = _nopUrlHelper.RouteUrl(NopRouteNames.General.SEARCH);

        //empty box: recent searches of this customer + popular searches
        if (term.Length == 0)
        {
            var recent = _catalogSettings.ShowSearchTermHistory
                ? (await _searchTermService.SearchTermHistoryItemsAsync((await _workContext.GetCurrentCustomerAsync()).Id, store.Id)).ToList()
                : [];

            return Json(new { term, recent, popular = await GetPopularTermsAsync(store.Id), products = Array.Empty<object>(), categories = Array.Empty<object>(), manufacturers = Array.Empty<object>(), totalCount = 0, searchUrl });
        }

        if (term.Length < _catalogSettings.ProductSearchTermMinimumLength)
            return Json(new { term, tooShort = true });

        //products: same query as the core autocomplete (honours the active search provider, e.g. Lucene)
        var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0
            ? _catalogSettings.ProductSearchAutoCompleteNumberOfProducts
            : 10;

        var categoryIds = new List<int>();
        if (cid > 0)
            categoryIds.AddRange([cid, .. await _categoryService.GetChildCategoryIdsAsync(cid, store.Id)]);

        var products = await _productService.SearchProductsAsync(0,
            pageSize: productNumber,
            categoryIds: categoryIds,
            storeId: store.Id,
            visibleIndividuallyOnly: true,
            keywords: term,
            languageId: (await _workContext.GetWorkingLanguageAsync()).Id);

        var productModels = await _productModelFactory.PrepareProductOverviewModelsAsync(products,
            preparePriceModel: _settings.ShowPrices,
            preparePictureModel: _catalogSettings.ShowProductImagesInSearchAutoComplete,
            productThumbPictureSize: _mediaSettings.AutoCompleteSearchThumbPictureSize);

        var productItems = new List<object>();
        foreach (var product in productModels)
        {
            productItems.Add(new
            {
                name = product.Name,
                url = await _nopUrlHelper.RouteGenericUrlAsync<Product>(new { SeName = product.SeName }),
                image = product.PictureModels?.FirstOrDefault()?.ImageUrl,
                price = product.ProductPrice?.Price,
                oldPrice = product.ProductPrice?.OldPrice
            });
        }

        //categories (published, store mapping and ACL are applied by the service)
        var categoryItems = new List<object>();
        if (_settings.ShowCategories && _settings.MaxCategories > 0)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(term, store.Id, pageSize: _settings.MaxCategories);
            foreach (var category in categories)
            {
                var parents = (await _categoryService.GetCategoryBreadCrumbAsync(category)).Where(c => c.Id != category.Id).ToList();
                var parentNames = new List<string>();
                foreach (var parent in parents)
                    parentNames.Add(await _localizationService.GetLocalizedAsync(parent, c => c.Name));

                categoryItems.Add(new
                {
                    name = await _localizationService.GetLocalizedAsync(category, c => c.Name),
                    url = await _nopUrlHelper.RouteGenericUrlAsync(category),
                    path = string.Join(" / ", parentNames)
                });
            }
        }

        //manufacturers
        var manufacturerItems = new List<object>();
        if (_settings.ShowManufacturers && _settings.MaxManufacturers > 0)
        {
            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(term, store.Id, pageSize: _settings.MaxManufacturers);
            foreach (var manufacturer in manufacturers)
            {
                manufacturerItems.Add(new
                {
                    name = await _localizationService.GetLocalizedAsync(manufacturer, m => m.Name),
                    url = await _nopUrlHelper.RouteGenericUrlAsync(manufacturer)
                });
            }
        }

        //popular searches are offered when nothing is found
        var nothingFound = productItems.Count == 0 && categoryItems.Count == 0 && manufacturerItems.Count == 0;

        return Json(new
        {
            term,
            products = productItems,
            categories = categoryItems,
            manufacturers = manufacturerItems,
            popular = nothingFound ? await GetPopularTermsAsync(store.Id) : [],
            totalCount = products.TotalCount,
            searchUrl
        });
    }

    #endregion
}
