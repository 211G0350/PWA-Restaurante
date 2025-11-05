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

        [HttpGet("crearNuevaOr")]
        public IActionResult CrearNuevaOr()
        {
            return View("~/wwwroot/Mesero/crearNuevaOr.cshtml");
        }

        [HttpGet("detalleordenDato/{id}")]
        public IActionResult DetalleordenDato(int id)
        {
            return View("~/wwwroot/Mesero/detalleordenDato.cshtml");
        }
    }
}
