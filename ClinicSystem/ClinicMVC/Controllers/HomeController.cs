using Microsoft.AspNetCore.Mvc;

namespace ClinicMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("ClinicManager"))
                    return RedirectToAction("Dashboard", "Manager");
                if (User.IsInRole("Doctor"))
                    return RedirectToAction("Dashboard", "Doctor");
                if (User.IsInRole("Receptionist"))
                    return RedirectToAction("Dashboard", "Receptionist");
                if (User.IsInRole("Patient"))
                    return RedirectToAction("Dashboard", "Patient");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Home/NotFound")]
        public override NotFoundResult NotFound()
        {
            Response.StatusCode = 404;
            return base.NotFound();
        }
    }
}
