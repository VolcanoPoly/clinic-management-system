using System.Net;
using System.Net.Http.Json;
using ClinicMVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ClinicMVC.Controllers
{
    public class LookupController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LookupController> _logger;

        public LookupController(IHttpClientFactory httpClientFactory, ILogger<LookupController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new PublicLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PublicLookupViewModel model)
        {
            model.HasSubmitted = true;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cpr = model.Cpr?.Trim() ?? string.Empty;
            var reference = model.Reference?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(cpr) || string.IsNullOrEmpty(reference))
            {
                model.LookupSuccess = false;
                model.Message = "Please enter both CPR and reference number.";
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ClinicApiClient");
                var url = $"api/appointments/lookup?cpr={WebUtility.UrlEncode(cpr)}&ref={WebUtility.UrlEncode(reference)}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var lookupResult = await response.Content.ReadFromJsonAsync<ClinicAPI.DTOs.PatientLookupResponseDto>();
                    if (lookupResult == null)
                    {
                        model.LookupSuccess = false;
                        model.Message = "Unable to read lookup response from the API.";
                        return View(model);
                    }

                    model.LookupResult = lookupResult;
                    model.LookupSuccess = lookupResult.Found;
                    model.Message = lookupResult.Message;
                    return View(model);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    model.LookupSuccess = false;
                    model.Message = "No patient found with the provided CPR and reference number.";
                    return View(model);
                }

                model.LookupSuccess = false;
                model.Message = "Unable to lookup appointments. Please try again later.";
                _logger.LogWarning("Lookup request failed: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling ClinicAPI for public appointment lookup");
                model.LookupSuccess = false;
                model.Message = "The lookup service is currently unavailable. Please try again later.";
                return View(model);
            }
        }
    }
}
