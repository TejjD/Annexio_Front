using System.Diagnostics;
using Annexio_Assessment.Models;
using Microsoft.AspNetCore.Mvc;
using Annexio_Frontend.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PagedList.Mvc;

namespace Annexio_Frontend.Controllers;

using Newtonsoft.Json;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static readonly HttpClient Client = new();

    private static List<Country>? _countriesList;
    private static Dictionary<string, List<string>>? _regionDictionary;
    private static Dictionary<string, long>? _regionPopulationDictionary;
    private static Dictionary<string, long>? _subRegionPopulationDictionary;
    private static Dictionary<string, List<string>>? _subRegionDictionary;
    private static Dictionary<string, List<string>>? _subRegionCountriesDictionary;
    
    public static List<string>? GetCountriesForRegion(string region)
    {
        return _regionDictionary?[region];
    }

    public static List<string>? GetSubRegionsForRegion(string region)
    {
        return _subRegionDictionary?[region];
    }

    public static Int64? GetRegionPopulation(string region)
    {
        return _regionPopulationDictionary?[region];
    }

    public static Int64? GetSubRegionPopulation(string region)
    {
        return _subRegionPopulationDictionary?[region];
    }

    public static List<string>? GetCountrySubRegionsForRegion(string subregion)
    {
        return _subRegionCountriesDictionary?[subregion];
    }

    public static string? GetCountryAlphaCode(string countryName)
    {
        var alp = (from country in _countriesList where country.name == countryName select country.alpha3Code).ToList();
        return alp[0];
    }

    public HomeController(ILogger<HomeController> logger)
    {
        GetCountries();
        _logger = logger;
    }

    private static void GetCountries()
    {
        var stringTask = Client.GetStringAsync("http://3.16.24.144:8080/Countries");

        var msg = stringTask.Result;

        _countriesList = (List<Country>)JsonConvert.DeserializeObject(msg, typeof(List<Country>))!;

        var regions = _countriesList.Select(x => x.region).Distinct();
        var subregions = _countriesList.Select(x => x.subregion).Distinct();

        _regionDictionary = new Dictionary<string, List<string>>();
        _subRegionDictionary = new Dictionary<string, List<string>>();
        _subRegionCountriesDictionary = new Dictionary<string, List<string>>();
        _regionPopulationDictionary = new Dictionary<string, Int64>();
        _subRegionPopulationDictionary = new Dictionary<string, Int64>();

        foreach (var subregion in subregions)
        {
            var countriesList = (from country in _countriesList where country.subregion == subregion select country.name)
                .ToList();
            _subRegionCountriesDictionary[subregion] = countriesList;

            var subRegionPopulationTotal =
                (from country in _countriesList where country.subregion == subregion select country.population);

            Int64 popTotal = 0;
            foreach (var pop in subRegionPopulationTotal)
            {
                popTotal += pop;
            }

            _subRegionPopulationDictionary[subregion] = popTotal;
        }

        foreach (var region in regions)
        {
            var countriesList = (from country in _countriesList where country.region == region select country.name)
                .ToList();
            var countriesPopulationTotal =
                (from country in _countriesList where country.region == region select country.population);
            var subregionsList = (from country in _countriesList where country.region == region select country.subregion)
                .ToList();
            var subRegionsDistinct = subregionsList.Select(x => x).Distinct();
            _regionDictionary[region] = countriesList;
            _subRegionDictionary[region] = new List<string>(subRegionsDistinct);
            Int64 popTotal = 0;
            foreach (var pop in countriesPopulationTotal)
            {
                popTotal += pop;
            }

            _regionPopulationDictionary[region] = popTotal;
        }
    }

    public IActionResult Index(int page = 0)
    {
        if (_countriesList == null) return View();
        const int pageSize = 15; // you can always do something more elegant to set this
        var orderedEnumerable = _countriesList.OrderBy(o => o.name);

        var count = orderedEnumerable.Count();

        var data = orderedEnumerable.Skip(page * pageSize).Take(pageSize).ToList();

        ViewBag.MaxPage = (count / pageSize) - (count % pageSize == 0 ? 1 : 0);
        ViewBag.Page = page;
        ViewBag.countriesList = _countriesList;
        
        //TODO: Uncomment to test Pagination (needs to be revised)
        //ViewBag.countriesList = data;
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}