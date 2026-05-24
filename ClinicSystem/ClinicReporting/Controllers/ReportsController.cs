using ClinicReporting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ClinicReporting.Controllers
{
    [Authorize(Roles = "ClinicManager")]
    public class ReportsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ReportsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> AppointmentStats(DateTime? from, DateTime? to)
        {
            var model = new AppointmentStatsViewModel
            {
                From = from ?? DateTime.UtcNow.AddDays(-30),
                To = to ?? DateTime.UtcNow
            };

            if (model.From > model.To)
            {
                model.ErrorMessage = "The start date must be before the end date.";
                return View(model);
            }

            var client = CreateApiClient();
            if (client == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("AppointmentStats", "Reports") });
            }

            try
            {
                var fromToken = Uri.EscapeDataString(model.From.ToString("o"));
                var toToken = Uri.EscapeDataString(model.To.ToString("o"));
                var response = await client.GetAsync($"api/reports/appointment-stats?from={fromToken}&to={toToken}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Logout", "Account");
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var report = JsonSerializer.Deserialize<AppointmentStatsViewModel>(content, _jsonOptions);

                if (report != null)
                {
                    report.From = model.From;
                    report.To = model.To;
                    return View(report);
                }

                model.ErrorMessage = "No report data was available for the selected date range.";
                return View(model);
            }
            catch (Exception)
            {
                model.ErrorMessage = "Unable to load appointment statistics. Please try again later.";
                return View(model);
            }
        }

        public async Task<IActionResult> DoctorUtilization(DateTime? from, DateTime? to)
        {
            var model = new DoctorUtilizationViewModel
            {
                From = from ?? DateTime.UtcNow.AddDays(-30),
                To = to ?? DateTime.UtcNow
            };

            if (model.From > model.To)
            {
                model.ErrorMessage = "The start date must be before the end date.";
                return View(model);
            }

            var client = CreateApiClient();
            if (client == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("DoctorUtilization", "Reports") });
            }

            try
            {
                var fromToken = Uri.EscapeDataString(model.From.ToString("o"));
                var toToken = Uri.EscapeDataString(model.To.ToString("o"));
                var response = await client.GetAsync($"api/reports/doctor-utilization?from={fromToken}&to={toToken}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Logout", "Account");
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var report = JsonSerializer.Deserialize<DoctorUtilizationViewModel>(content, _jsonOptions);

                if (report != null)
                {
                    report.From = model.From;
                    report.To = model.To;
                    return View(report);
                }

                model.ErrorMessage = "No report data was available for the selected date range.";
                return View(model);
            }
            catch (Exception)
            {
                model.ErrorMessage = "Unable to load doctor utilization data. Please try again later.";
                return View(model);
            }
        }

        public async Task<IActionResult> CancellationRates(DateTime? from, DateTime? to)
        {
            var model = new CancellationRatesViewModel
            {
                From = from ?? DateTime.UtcNow.AddDays(-30),
                To = to ?? DateTime.UtcNow
            };

            if (model.From > model.To)
            {
                model.ErrorMessage = "The start date must be before the end date.";
                return View(model);
            }

            var client = CreateApiClient();
            if (client == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("CancellationRates", "Reports") });
            }

            try
            {
                var fromToken = Uri.EscapeDataString(model.From.ToString("o"));
                var toToken = Uri.EscapeDataString(model.To.ToString("o"));
                var response = await client.GetAsync($"api/reports/cancellation-rates?from={fromToken}&to={toToken}");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToAction("Logout", "Account");
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var report = JsonSerializer.Deserialize<CancellationRatesViewModel>(content, _jsonOptions);

                if (report != null)
                {
                    report.From = model.From;
                    report.To = model.To;
                    return View(report);
                }

                model.ErrorMessage = "No report data was available for the selected date range.";
                return View(model);
            }
            catch (Exception)
            {
                model.ErrorMessage = "Unable to load cancellation rate data. Please try again later.";
                return View(model);
            }
        }

        private HttpClient? CreateApiClient()
        {
            var token = HttpContext.Session.GetString("ReportingJwtToken");
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient("ClinicApiClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
