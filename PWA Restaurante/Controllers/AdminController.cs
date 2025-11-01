using Microsoft.AspNetCore.Mvc;

namespace PWA_Restaurante.Controllers
{
	[Route("Admin")]
	public class AdminController : Controller
	{
		[HttpGet("panelAdmin")]
		public IActionResult PanelAdmin()
		{
			return View("~/wwwroot/Admin/panelAdmin.cshtml");
		}

		[HttpGet("usuariosadmin")]
		public IActionResult UsuariosAdmin()
		{
			return View("~/wwwroot/Admin/usuariosadmin.cshtml");
		}

		[HttpGet("adminProducts")]
		public IActionResult AdminProducts()
		{
			return View("~/wwwroot/Admin/adminProducts.cshtml");
		}
	}
}

