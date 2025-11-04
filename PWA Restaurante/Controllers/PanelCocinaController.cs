using Microsoft.AspNetCore.Mvc;

namespace PWA_Restaurante.Controllers
{
    [Route("Cociner")]
    public class PanelCocinerController : Controller
    {
        [HttpGet("panelCocina")]
        public IActionResult PanelCocina()
        {
            return View("~/wwwroot/Cociner/panelCocina.cshtml");
        }
    }
}
