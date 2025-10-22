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

		[HttpGet("EnPreparacion")]
		public IActionResult ObtenerPedidosEnPreparacion()
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "En Preparacion")
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

		[HttpGet("Listo")]
		public IActionResult ObtenerPedidosListo()
		{
			var pedidos = _pedidoRepository.GetAll()
				.Where(p => p.Estado == "Listo")
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

		[HttpPost("AgregarPedido")]
		public IActionResult AgregarPedido(AgregarPedidoDTO dto)
		{
			if (dto.Detalles == null || !dto.Detalles.Any())
			{
				return BadRequest("El pedido debe tener al menos un producto");
			}

			if (dto.NumMesa <= 0)
			{
				return BadRequest("El número de mesa debe ser mayor a 0");
			}

	
			var usuario = _usuarioRepository.GetAll().FirstOrDefault(u => u.Id == dto.UsuarioId);
			if (usuario == null)
			{
				return BadRequest("El usuario no existe");
			}

			var pedido = new Pedidos
			{
				NumMesa = dto.NumMesa,
				NotasEspeciales = dto.NotasEspeciales,
				UsuarioId = dto.UsuarioId,
				TomadoEn = DateTime.Now,
				Estado = dto.Estado,
				PrecioTotal = 0 // Se calculará después
			};

			_pedidoRepository.Insert(pedido);

			// Detalles del pedido osi
			decimal precioTotal = 0;
			foreach (var detalleDto in dto.Detalles)
			{
				var producto = _productoRepository.GetAll().FirstOrDefault(p => p.Id == detalleDto.ProductoId);
				if (producto == null)
				{
					return BadRequest($"El producto con ID {detalleDto.ProductoId} no existe");
				}

				var detalle = new PedidoDetalles
				{
					PedidoId = pedido.Id,
					ProductoId = detalleDto.ProductoId,
					Cantidad = detalleDto.Cantidad,
					PrecioUnitario = detalleDto.PrecioUnitario,
					Subtotal = detalleDto.Cantidad * detalleDto.PrecioUnitario
				};

				_detalleRepository.Insert(detalle);
				precioTotal += detalle.Subtotal ?? 0;
			}

			// todos los subtotales dan el precio total
			pedido.PrecioTotal = precioTotal;
			_pedidoRepository.Update(pedido);

			return Ok(new { message = "Pedido agregado exitosamente", id = pedido.Id });
		}

		[HttpPut("EditarPedido")]
		public IActionResult EditarPedido(EditarPedidoDTO dto)
		{
			var pedidoExistente = _pedidoRepository.GetAll().FirstOrDefault(p => p.Id == dto.Id);
			if (pedidoExistente == null)
			{
				return NotFound("Pedido no encontrado");
			}

			// Solo podremos editar pedidos con estado "Pendiente" o "Enviado"
			if (pedidoExistente.Estado != "Pendiente" && pedidoExistente.Estado != "Enviado")
			{
				return BadRequest("Un pedido iniciado ya no se puede editar");
			}

			if (dto.Detalles == null || !dto.Detalles.Any())
			{
				return BadRequest("El pedido debe tener al menos un producto");
			}

			if (dto.NumMesa <= 0)
			{
				return BadRequest("El número de mesa debe ser mayor a 0");
			}

			var usuario = _usuarioRepository.GetAll().FirstOrDefault(u => u.Id == dto.UsuarioId);
			if (usuario == null)
			{
				return BadRequest("El usuario no existe");
			}

			var detallesExistentes = _detalleRepository.GetAll().Where(d => d.PedidoId == dto.Id).ToList();
			foreach (var detalle in detallesExistentes)
			{
				_detalleRepository.Delete(detalle);
			}

			// reemplazar detalles anteriores por los nuevos
			pedidoExistente.NumMesa = dto.NumMesa;
			pedidoExistente.NotasEspeciales = dto.NotasEspeciales;
			pedidoExistente.UsuarioId = dto.UsuarioId;
			pedidoExistente.Estado = dto.Estado;
			pedidoExistente.PrecioTotal = 0; 
			decimal precioTotal = 0;
			foreach (var detalleDto in dto.Detalles)
			{
				var producto = _productoRepository.GetAll().FirstOrDefault(p => p.Id == detalleDto.ProductoId);
				if (producto == null)
				{
					return BadRequest($"El producto con ID {detalleDto.ProductoId} no existe");
				}

				var detalle = new PedidoDetalles
				{
					PedidoId = pedidoExistente.Id,
					ProductoId = detalleDto.ProductoId,
					Cantidad = detalleDto.Cantidad,
					PrecioUnitario = detalleDto.PrecioUnitario,
					Subtotal = detalleDto.Cantidad * detalleDto.PrecioUnitario
				};

				_detalleRepository.Insert(detalle);
				precioTotal += detalle.Subtotal ?? 0;
			}
			pedidoExistente.PrecioTotal = precioTotal;
			_pedidoRepository.Update(pedidoExistente);

			return Ok(new { message = "Pedido editado exitosamente" });
		}

		[HttpGet("ObtenerParaCancelar/{id}")]
		public IActionResult ObtenerPedidoParaCancelar(int id)
		{
			var pedido = _pedidoRepository.GetAll().FirstOrDefault(p => p.Id == id);
			if (pedido == null)
			{
				return NotFound("Pedido no encontrado");
			}

			// Solo se pueden cancelar pedidos con estado "Pendiente" o "Enviado"
			if (pedido.Estado != "Pendiente" && pedido.Estado != "Enviado")
			{
				return BadRequest("No se pueden cancelar pedidos ya iniciados");
			}

			var pedidoParaCancelar = new CancelarPedidoDTO
			{
				Id = pedido.Id,
				NumMesa = pedido.NumMesa,
				TomadoEn = pedido.TomadoEn,
				PrecioTotal = pedido.PrecioTotal
			};

			return Ok(pedidoParaCancelar);
		}

		[HttpDelete("CancelarPedido/{id}")]
		public IActionResult CancelarPedido(int id)
		{
			var pedido = _pedidoRepository.GetAll().FirstOrDefault(p => p.Id == id);
			if (pedido == null)
			{
				return NotFound("Pedido no encontrado");
			}

			if (pedido.Estado != "Pendiente" && pedido.Estado != "Enviado")
			{
				return BadRequest("No se pueden cancelar pedidos ya iniciados");
			}

			var detalles = _detalleRepository.GetAll().Where(d => d.PedidoId == id).ToList();
			foreach (var detalle in detalles)
			{
				_detalleRepository.Delete(detalle);
			}
			_pedidoRepository.Delete(pedido);

			return Ok(new { message = "Pedido cancelado y eliminado exitosamente" });
		}
	}
}