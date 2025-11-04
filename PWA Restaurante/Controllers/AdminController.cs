using AspNetCoreGeneratedDocument;
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

		[HttpGet("EditProducto/{id}")]
		public IActionResult EditProducto(int id)
		{
			return View("~/wwwroot/Admin/EditProducto.cshtml");
		}

		[HttpGet("eliminarProducto/{id}")]
		public IActionResult EliminarProducto(int id)
		{
			return View("~/wwwroot/Admin/eliminarProducto.cshtml");
		}

		[HttpGet("aggProductos")]
		public IActionResult AggProductos()
		{
			return View("~/wwwroot/Admin/aggProductos.cshtml");
		}

		[HttpGet("EditaUsuario/{id}")]
		public IActionResult EditaUsuario(int id)
		{
			return View("~/wwwroot/Admin/EditaUsuario.cshtml");
		}

		[HttpGet("EliminaciondUsuario/{id}")]
		public IActionResult EliminaciondUsuario(int id)
		{
			return View("~/wwwroot/Admin/EliminaciondUsuarios.cshtml");
		}

		[HttpGet("AggUsuario")]
		public IActionResult AggUsuario()
		{
			return View("~/wwwroot/Admin/AggUsuario.cshtml");
		}
	}
}

