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

		[HttpGet("EditarPendiente/{id}")]
		public IActionResult EditarPendiente(int id)
		{
			ViewData["PedidoId"] = id;
			return View("~/wwwroot/Mesero/EditarPendiente.cshtml");
		}

		[HttpGet("EliminarPendiente/{id}")]
		public IActionResult EliminarPendiente(int id)
		{
			ViewData["PedidoId"] = id;
			return View("~/wwwroot/Mesero/EliminarPeniente.cshtml");
		}
    }
}
