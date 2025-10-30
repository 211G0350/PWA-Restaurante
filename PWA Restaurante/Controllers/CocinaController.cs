using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;

namespace PWA_Restaurante.Controllers
{
	[Authorize(Roles = "Cocinero")]
	[Route("api/[controller]")]
	[ApiController]
	public class CocinaController : ControllerBase
	{
		private readonly Repository<Pedidos> _pedidoRepository;
		private readonly Repository<PedidoDetalles> _detalleRepository;
		private readonly Repository<Productos> _productoRepository;
		private readonly Repository<Usuarios> _usuarioRepository;

		public CocinaController(
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

		[HttpGet("Enviados")]
		public IActionResult ObtenerPedidosEnviados()
		{
			var pedidos = _pedidoRepository.GetQueryable()
				.Where(p => p.Estado == "enviado")
				.Join(_usuarioRepository.GetQueryable(),
					p => p.UsuarioId,
					u => u.Id,
					(p, u) => new PedidoResumenCocinaDTO
					{
						Id = p.Id,
						NumMesa = p.NumMesa,
						TomadoEn = p.TomadoEn,
						PrecioTotal = p.PrecioTotal,
						Estado = p.Estado,
						UsuarioNombre = u.Nombre
					})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("EnPreparacion")]
		public IActionResult ObtenerPedidosEnPreparacion()
		{
			var pedidos = _pedidoRepository.GetQueryable()
				.Where(p => p.Estado == "en preparacion")
				.Join(_usuarioRepository.GetQueryable(),
					p => p.UsuarioId,
					u => u.Id,
					(p, u) => new PedidoResumenCocinaDTO
					{
						Id = p.Id,
						NumMesa = p.NumMesa,
						TomadoEn = p.TomadoEn,
						PrecioTotal = p.PrecioTotal,
						Estado = p.Estado,
						UsuarioNombre = u.Nombre
					})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("Listo")]
		public IActionResult ObtenerPedidosListo()
		{
			var pedidos = _pedidoRepository.GetQueryable()
				.Where(p => p.Estado == "listo")
				.Join(_usuarioRepository.GetQueryable(),
					p => p.UsuarioId,
					u => u.Id,
					(p, u) => new PedidoResumenCocinaDTO
					{
						Id = p.Id,
						NumMesa = p.NumMesa,
						TomadoEn = p.TomadoEn,
						PrecioTotal = p.PrecioTotal,
						Estado = p.Estado,
						UsuarioNombre = u.Nombre
					})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpGet("Todos")]
		public IActionResult ObtenerTodosLosPedidos()
		{
			var pedidos = _pedidoRepository.GetQueryable()
				.Where(p => p.Estado != "pendiente")
				.Join(_usuarioRepository.GetQueryable(),
					p => p.UsuarioId,
					u => u.Id,
					(p, u) => new PedidoResumenCocinaDTO
					{
						Id = p.Id,
						NumMesa = p.NumMesa,
						TomadoEn = p.TomadoEn,
						PrecioTotal = p.PrecioTotal,
						Estado = p.Estado,
						UsuarioNombre = u.Nombre
					})
				.OrderByDescending(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

		[HttpPut("ActualizarEstado/{id}")]
		public IActionResult ActualizarEstado(int id)
		{
			var pedido = _pedidoRepository.GetByIdWithTracking(id);
			if (pedido == null)
			{
				return NotFound("Pedido no encontrado");
			}

			string nuevoEstado;
			string mensaje;

			switch (pedido.Estado)
			{
				case "enviado":
					nuevoEstado = "en preparacion";
					mensaje = "Pedido enviado a preparación";
					break;
				case "en preparacion":
					nuevoEstado = "listo";
					mensaje = "Pedido marcado como listo";
					break;
				case "listo":
					return BadRequest("El pedido ya está listo, no se puede cambiar el estado");
				case "pendiente":
					return BadRequest("Los pedidos pendientes no se pueden procesar desde cocina");
				default:
					return BadRequest($"Estado '{pedido.Estado}' no válido para actualización desde cocina");
			}

			pedido.Estado = nuevoEstado;
			_pedidoRepository.Update(pedido);

			return Ok(new
			{
				message = mensaje,
				pedidoId = pedido.Id,
				estadoAnterior = pedido.Estado == "en preparacion" ? "enviado" : "en preparacion",
				nuevoEstado = nuevoEstado,
				numMesa = pedido.NumMesa
			});
		}

		[HttpGet("PorEstado/{estado}")]
		public IActionResult ObtenerPedidosPorEstado(string estado)
		{
		
			if (estado.ToLower() == "pendiente")
			{
				return BadRequest("Los pedidos pendientes no se pueden ver desde cocina");
			}

			var pedidos = _pedidoRepository.GetQueryable()
				.Where(p => p.Estado.ToLower() == estado.ToLower())
				.Join(_usuarioRepository.GetQueryable(),
					p => p.UsuarioId,
					u => u.Id,
					(p, u) => new PedidoResumenCocinaDTO
					{
						Id = p.Id,
						NumMesa = p.NumMesa,
						TomadoEn = p.TomadoEn,
						PrecioTotal = p.PrecioTotal,
						Estado = p.Estado,
						UsuarioNombre = u.Nombre
					})
				.OrderBy(p => p.TomadoEn)
				.ToList();

			return Ok(pedidos);
		}

	
	}
}
