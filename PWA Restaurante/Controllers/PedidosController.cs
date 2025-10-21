using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;

namespace PWA_Restaurante.Controllers
{
	[Authorize(Roles = "Mesero")]
	[Route("api/[controller]")]
	[ApiController]
	public class PedidosController : ControllerBase
	{
		private readonly Repository<Pedidos> _pedidoRepository;
		private readonly Repository<PedidoDetalles> _detalleRepository;
		private readonly Repository<Productos> _productoRepository;
		private readonly Repository<Usuarios> _usuarioRepository;

		public PedidosController(
			Repository<Pedidos> pedidoRepository,
			Repository<PedidoDetalles> detalleRepository,
			Repository<Productos> productoRepository,
			Repository<Usuarios> usuarioRepository)
		{
			_pedidoRepository = pedidoRepository;
			_detalleRepository = detalleRepository;
			_productoRepository = productoRepository;
			_usuarioRepository = usuarioRepository;
		}

		[HttpGet("Pendientes")]
		public IActionResult ObtenerPedidosPendientes()
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "Pendiente")
				.Select(p => new PedidoResumen_DTO
				{
					Id = p.Id,
					NumMesa = p.NumMesa,
					TomadoEn = p.TomadoEn,
					PrecioTotal = p.PrecioTotal,
					Estado = p.Estado
				})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("Enviados")]
		public IActionResult ObtenerPedidosEnviados()
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "Enviado")
				.Select(p => new PedidoResumen_DTO
				{
					Id = p.Id,
					NumMesa = p.NumMesa,
					TomadoEn = p.TomadoEn,
					PrecioTotal = p.PrecioTotal,
					Estado = p.Estado
				})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("Todos")]
		public IActionResult ObtenerTodosLosPedidos()
		{
			var pedidos = _pedidoRepository.GetAll()
				.Select(p => new PedidoResumen_DTO
				{
					Id = p.Id,
					NumMesa = p.NumMesa,
					TomadoEn = p.TomadoEn,
					PrecioTotal = p.PrecioTotal,
					Estado = p.Estado
				})
				.OrderByDescending(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("VerDetalles/{id}")]
		public IActionResult VerDetallesPedido(int id)
		{
			var pedido = _pedidoRepository.GetAll()
				.FirstOrDefault(p => p.Id == id);

			if (pedido == null)
			{
				return NotFound("Pedido no encontrado");
			}

			var usuario = _usuarioRepository.GetAll()
				.FirstOrDefault(u => u.Id == pedido.UsuarioId);

			var detalles = _detalleRepository.GetAll()
				.Where(d => d.PedidoId == id)
				.Join(_productoRepository.GetAll(),
					d => d.ProductoId,
					p => p.Id,
					(d, p) => new PedidoDetalleDTO
					{
						Id = d.Id,
						NombreProducto = p.Nombre,
						Cantidad = d.Cantidad,
						PrecioUnitario = d.PrecioUnitario,
						Subtotal = d.Cantidad * d.PrecioUnitario
					})
				.ToList();

			var pedidoCompleto = new PedidoDetalleCompletoDTO
			{
				Id = pedido.Id,
				NumMesa = pedido.NumMesa,
				NotasEspeciales = pedido.NotasEspeciales,
				UsuarioNombre = usuario?.Nombre ?? "Usuario no encontrado",
				TomadoEn = pedido.TomadoEn,
				Estado = pedido.Estado,
				PrecioTotal = pedido.PrecioTotal,
				Detalles = detalles
			};

			return Ok(pedidoCompleto);
		}

		[HttpGet("PorMesa/{numMesa}")]
		public IActionResult ObtenerPedidosPorMesa(int numMesa)
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.NumMesa == numMesa)
				.Select(p => new PedidoResumen_DTO
				{
					Id = p.Id,
					NumMesa = p.NumMesa,
					TomadoEn = p.TomadoEn,
					PrecioTotal = p.PrecioTotal,
					Estado = p.Estado
				})
				.OrderByDescending(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("PorEstado/{estado}")]
		public IActionResult ObtenerPedidosPorEstado(string estado)
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado.ToLower() == estado.ToLower())
				.Select(p => new PedidoResumen_DTO
				{
					Id = p.Id,
					NumMesa = p.NumMesa,
					TomadoEn = p.TomadoEn,
					PrecioTotal = p.PrecioTotal,
					Estado = p.Estado
				})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}
	}
}
