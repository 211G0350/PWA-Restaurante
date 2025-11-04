using Microsoft.AspNetCore.Mvc;

namespace PWA_Restaurante.Controllers
{
    [Route("Mesero")]
    public class MeseroController : Controller
    {
        [HttpGet("panelMesero")]
        public IActionResult PanelMesero()
        {
            return View("~/wwwroot/Mesero/panelMesero.cshtml");
        }
    }
}
