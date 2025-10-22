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
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "Enviado")
				.Join(_usuarioRepository.GetAll(),
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
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "En Preparacion")
				.Join(_usuarioRepository.GetAll(),
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
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "Listo")
				.Join(_usuarioRepository.GetAll(),
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
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado != "Pendiente")
				.Join(_usuarioRepository.GetAll(),
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

		[HttpPut("ActualizarEstado/{id}")]
		public IActionResult ActualizarEstado(int id)
		{
			var pedido = _pedidoRepository.GetAll().FirstOrDefault(p => p.Id == id);
			if (pedido == null)
			{
				return NotFound("Pedido no encontrado");
			}

			string nuevoEstado;
			string mensaje;

			switch (pedido.Estado)
			{
				case "Enviado":
					nuevoEstado = "En Preparacion";
					mensaje = "Pedido enviado a preparación";
					break;
				case "En Preparacion":
					nuevoEstado = "Listo";
					mensaje = "Pedido marcado como listo";
					break;
				case "Listo":
					return BadRequest("El pedido ya está listo, no se puede cambiar el estado");
				case "Pendiente":
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
				estadoAnterior = pedido.Estado == "En Preparacion" ? "Enviado" : "En Preparacion",
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

			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado.ToLower() == estado.ToLower())
				.Join(_usuarioRepository.GetAll(),
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
