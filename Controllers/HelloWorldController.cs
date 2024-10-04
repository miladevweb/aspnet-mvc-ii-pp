using Microsoft.AspNetCore.Mvc;

namespace MvcPractice.Controllers;

public class HelloWorldController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // In program.cs, we have a default route with "id" as Param Name that's why "name" is a Query Param
    public IActionResult Welcome(string name="DEFAULT_NAME", int numTimes = 1, int ID = 10)
    {
        ViewData["NumTimes"] = numTimes;
        ViewData["Message"] = $"Hello {name}, your ID is {ID}";
        return View();
    }
}
