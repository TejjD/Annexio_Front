using System.Diagnostics;
using Annexio_Assessment.Models;
using Microsoft.AspNetCore.Mvc;
using Annexio_Frontend.Models;

namespace Annexio_Frontend.Controllers;

using Newtonsoft.Json;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static readonly HttpClient Client = new();

    private static List<Country>? CountriesList;
    private static Dictionary<string, List<string>> RegionDictionary;
    private static Dictionary<string, Int64> RegionPopulationDictionary;
    private static Dictionary<string, Int64> SubRegionPopulationDictionary;
    private static Dictionary<string, List<string>> SubRegionDictionary;
    private static Dictionary<string, List<string>> SubRegionCountriesDictionary;

    public static List<string>? GetCountriesForRegion(string region)
    {
        return RegionDictionary?[region];
    }

    public static List<string>? GetSubRegionsForRegion(string region)
    {
        return SubRegionDictionary?[region];
    }

    public static Int64? GetRegionPopulation(string region)
    {
        return RegionPopulationDictionary?[region];
    }

    public static Int64? GetSubRegionPopulation(string region)
    {
        return SubRegionPopulationDictionary?[region];
    }

    public static List<string>? GetCountrySubRegionsForRegion(string subregion)
    {
        return SubRegionCountriesDictionary?[subregion];
    }

    public static string? GetCountryAlphaCode(string countryName)
    {
        var alp = (from country in CountriesList where country.name == countryName select country.alpha3Code).ToList();
        return alp[0];
    }

    public HomeController(ILogger<HomeController> logger)
    {
        GetCountries();
        _logger = logger;
    }

    private static void GetCountries()
    {
        var stringTask = Client.GetStringAsync("https://localhost:7040/Countries");

        var msg = stringTask.Result;

        CountriesList = (List<Country>)JsonConvert.DeserializeObject(msg, typeof(List<Country>))!;

        var regions = CountriesList.Select(x => x.region).Distinct();
        var subregions = CountriesList.Select(x => x.subregion).Distinct();

        RegionDictionary = new Dictionary<string, List<string>>();
        SubRegionDictionary = new Dictionary<string, List<string>>();
        SubRegionCountriesDictionary = new Dictionary<string, List<string>>();
        RegionPopulationDictionary = new Dictionary<string, Int64>();
        SubRegionPopulationDictionary = new Dictionary<string, Int64>();

        foreach (var subregion in subregions)
        {
            var countriesList = (from country in CountriesList where country.subregion == subregion select country.name)
                .ToList();
            SubRegionCountriesDictionary[subregion] = countriesList;

            var subRegionPopulationTotal =
                (from country in CountriesList where country.subregion == subregion select country.population);

            Int64 popTotal = 0;
            foreach (var pop in subRegionPopulationTotal)
            {
                popTotal += pop;
            }

            SubRegionPopulationDictionary[subregion] = popTotal;
        }

        foreach (var region in regions)
        {
            var countriesList = (from country in CountriesList where country.region == region select country.name)
                .ToList();
            var countriesPopulationTotal =
                (from country in CountriesList where country.region == region select country.population);
            var subregionsList = (from country in CountriesList where country.region == region select country.subregion)
                .ToList();
            var subRegionsDistinct = subregionsList.Select(x => x).Distinct();
            RegionDictionary[region] = countriesList;
            SubRegionDictionary[region] = new List<string>(subRegionsDistinct);
            Int64 popTotal = 0;
            foreach (var pop in countriesPopulationTotal)
            {
                popTotal += pop;
            }

            RegionPopulationDictionary[region] = popTotal;
        }
    }

    public IActionResult Index()
    {
        if (CountriesList != null) ViewBag.countriesList = CountriesList;
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