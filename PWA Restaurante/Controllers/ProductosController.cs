using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;
using PWA_Restaurante.Models.Validators;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using PWA_Restaurante.Services;

namespace PWA_Restaurante.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductosController : ControllerBase
	{
		private readonly Repository<Productos> _repository;
		private readonly Repository<PedidoDetalles> _pedidoDetallesRepository;
		private readonly ProductosValidator _validator;
		private readonly IWebHostEnvironment _hostEnvironment;
		private readonly IHubContext<PedidosHub> _hubContext;

		public ProductosController(Repository<Productos> repository, Repository<PedidoDetalles> pedidoDetallesRepository, ProductosValidator validator, IWebHostEnvironment hostEnvironment, IHubContext<PedidosHub> hubContext)
		{
			_repository = repository;
			_pedidoDetallesRepository = pedidoDetallesRepository;
			_validator = validator;
			_hostEnvironment = hostEnvironment;
			_hubContext = hubContext;
		}

		[HttpGet("Todos")]
		public IActionResult ObtenerTodosLosProductos()
		{
			var productos = _repository.GetAll()
				.Where(p => p.Activo == true)
				.Select(p => new ProductoDTO
				{
					Id = p.Id,
					Nombre = p.Nombre,
					Precio = p.Precio,
					Categoria = p.Categoria,
					Descripcion = p.Descripcion,
					TiempoPreparacion = p.TiempoPreparacion,
					ImagenProducto = p.ImagenProducto,
					Activo = p.Activo
				})
				.ToList();

			return Ok(productos);
		}

		[HttpGet("TodosAdmin")]
		[Authorize(Roles = "Admin")]
		public IActionResult ObtenerTodosLosProductosAdmin()
		{
			var productos = _repository.GetAll()
				.Select(p => new ProductoDTO
				{
					Id = p.Id,
					Nombre = p.Nombre,
					Precio = p.Precio,
					Categoria = p.Categoria,
					Descripcion = p.Descripcion,
					TiempoPreparacion = p.TiempoPreparacion,
					ImagenProducto = p.ImagenProducto,
					Activo = p.Activo
				})
				.ToList();

			return Ok(productos);
		}

		[HttpGet("ObtenerPorId/{id}")]
		public IActionResult ObtenerProductoPorId(int id)
		{
			var producto = _repository.GetAll().FirstOrDefault(p => p.Id == id);
			if (producto == null)
			{
				return NotFound("Producto no encontrado");
			}

			var productoDTO = new ProductoDTO
			{
				Id = producto.Id,
				Nombre = producto.Nombre,
				Precio = producto.Precio,
				Categoria = producto.Categoria,
				Descripcion = producto.Descripcion,
				TiempoPreparacion = producto.TiempoPreparacion,
				ImagenProducto = producto.ImagenProducto,
				Activo = producto.Activo
			};

			return Ok(productoDTO);
		}

		[HttpPost("AgregarProducto")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> AgregarProducto([FromForm] AgregarProductoDTO dto, IFormFile archivo)
		{
			if (_validator.ValidateAgregar(dto, out List<string> errores))
			{
				var categoriaExistente = _repository.GetAll()
					.Any(p => p.Categoria.ToLower() == dto.Categoria.ToLower());
				
				var producto = new Productos
				{
					Nombre = dto.Nombre,
					Precio = dto.Precio,
					Categoria = dto.Categoria, 
					Descripcion = dto.Descripcion,
					TiempoPreparacion = dto.TiempoPreparacion,
					ImagenProducto = dto.ImagenProducto,
					Activo = dto.Activo
				};

				_repository.Insert(producto);
				
				if (archivo != null)
				{
					if (archivo.ContentType != "image/jpeg")
					{
						return BadRequest(new { message = "Solo están permitidas imagenes .jpg" });
					}

					if (archivo.Length > 1024 * 1024 * 5)
					{
						return BadRequest(new { message = "No se permiten imagenes mayores a 5MB" });
					}

					var ruta = Path.Combine(_hostEnvironment.WebRootPath, "Img", producto.Id + ".jpg");
					
					var directory = Path.GetDirectoryName(ruta);
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					using (FileStream fs = new FileStream(ruta, FileMode.Create))
					{
						archivo.CopyTo(fs);
					}
				}
				
				var mensaje = categoriaExistente 
					? "Producto agregado exitosamente" 
					: $"Producto agregado exitosamente. Nueva categoría '{dto.Categoria}' creada automáticamente";
				
				await _hubContext.Clients.All.SendAsync("ProductoAgregado", new
				{
					productoId = producto.Id,
					nombre = producto.Nombre,
					categoria = producto.Categoria,
					precio = producto.Precio,
					activo = producto.Activo
				});
				
				return Ok(new { message = mensaje, id = producto.Id });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[HttpPut("Editar/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> EditarProducto(int id, [FromForm] EditarProductoDTO dto, IFormFile? archivo)
		{
			if (dto == null || string.IsNullOrEmpty(dto.Nombre))
			{
				var form = HttpContext.Request.Form;
				dto = new EditarProductoDTO
				{
					Id = id,
					Nombre = form["Nombre"].ToString() ?? string.Empty,
					Categoria = form["Categoria"].ToString() ?? string.Empty,
					Descripcion = form["Descripcion"].ToString(),
					Precio = decimal.TryParse(form["Precio"], out decimal precio) ? precio : 0,
					TiempoPreparacion = int.TryParse(form["TiempoPreparacion"], out int tiempo) ? tiempo : 0,
					ImagenProducto = form["ImagenProducto"].ToString(),
					Activo = form["Activo"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase)
				};
				archivo = form.Files.GetFile("archivo");
			}
			else
			{
				dto.Id = id;
				if (archivo == null)
				{
					archivo = HttpContext.Request.Form.Files.GetFile("archivo");
				}
			}

			if (archivo == null && ModelState.ContainsKey("archivo"))
			{
				ModelState.Remove("archivo");
			}
			var relevantErrors = ModelState
				.Where(x => x.Key != "archivo" && x.Value?.Errors.Count > 0)
				.SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
				.ToList();
			
			if (relevantErrors.Any())
			{
				return BadRequest(new { message = "Error de validación del modelo", errors = relevantErrors });
			}
			
			if (_validator.ValidateEditar(dto, out List<string> errores))
			{
				var productoExistente = _repository.GetByIdWithTracking(id);
				
				if (productoExistente == null)
				{
					return NotFound("Producto no encontrado");
				}

				productoExistente.Nombre = dto.Nombre;
				productoExistente.Precio = dto.Precio;
				productoExistente.Categoria = dto.Categoria;
				productoExistente.Descripcion = dto.Descripcion;
				productoExistente.TiempoPreparacion = dto.TiempoPreparacion;
				productoExistente.ImagenProducto = dto.ImagenProducto;
				productoExistente.Activo = dto.Activo;

				_repository.Update(productoExistente);

				if (archivo != null)
				{
					if (archivo.ContentType != "image/jpeg")
					{
						return BadRequest(new { message = "Solo están permitidas imagenes .jpg" });
					}

					if (archivo.Length > 1024 * 1024 * 5)
					{
						return BadRequest(new { message = "No se permiten imagenes mayores a 5MB" });
					}

					var ruta = Path.Combine(_hostEnvironment.WebRootPath, "Img", dto.Id + ".jpg");
					var directory = Path.GetDirectoryName(ruta);
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					using (FileStream fs = new FileStream(ruta, FileMode.Create))
					{
						archivo.CopyTo(fs);
					}
				}

				await _hubContext.Clients.All.SendAsync("ProductoEditado", new
				{
					productoId = productoExistente.Id,
					nombre = productoExistente.Nombre,
					categoria = productoExistente.Categoria,
					precio = productoExistente.Precio,
					activo = productoExistente.Activo
				});

				return Ok(new { message = "Producto actualizado exitosamente" });
			}
			else
			{
				return BadRequest(new { message = "Error de validación", errors = errores });
			}
		}

		[HttpDelete("EliminarProducto/{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> EliminarProducto(int id)
		{
			if (!_validator.ValidateEliminar(id, out List<string> errores))
			{
				return BadRequest(errores);
			}

			var producto = _repository.GetByIdWithTracking(id);
			if (producto == null)
			{
				return NotFound("Producto no encontrado");
			}

			var tienePedidos = _pedidoDetallesRepository.GetAll()
				.Any(pd => pd.ProductoId == id);

			if (tienePedidos)
			{
				return BadRequest(new { message = "No se puede eliminar el producto porque está asociado a uno o más pedidos." });
			}

			var categoriaEliminada = producto.Categoria;
			_repository.Delete(producto.Id);

			try
			{
				var rutaImagen = Path.Combine(_hostEnvironment.WebRootPath, "Img", $"{producto.Id}.jpg");
				if (System.IO.File.Exists(rutaImagen))
				{
					System.IO.File.Delete(rutaImagen);
				}
			}
			catch
			{

			}

			await _hubContext.Clients.All.SendAsync("ProductoEliminado", new
			{
				productoId = producto.Id,
				categoria = categoriaEliminada
			});

			return Ok(new { message = "Producto eliminado permanentemente" });
		}


		[HttpGet("PorCategoria/{categoria}")]
		public IActionResult ObtenerProductosPorCategoria(string categoria)
		{
			var productos = _repository.GetAll()
				.Where(p => p.Categoria.ToLower() == categoria.ToLower() && p.Activo == true)
				.Select(p => new ProductoDTO
				{
					Id = p.Id,
					Nombre = p.Nombre,
					Precio = p.Precio,
					Categoria = p.Categoria,
					Descripcion = p.Descripcion,
					TiempoPreparacion = p.TiempoPreparacion,
					ImagenProducto = p.ImagenProducto,
					Activo = p.Activo
				})
				.ToList();

			return Ok(productos);
		}

		[HttpGet("PorCategoriaAdmin/{categoria}")]
		[Authorize(Roles = "Admin")]
		public IActionResult ObtenerProductosPorCategoriaAdmin(string categoria)
		{
			var productos = _repository.GetAll()
				.Where(p => p.Categoria.ToLower() == categoria.ToLower())
				.Select(p => new ProductoDTO
				{
					Id = p.Id,
					Nombre = p.Nombre,
					Precio = p.Precio,
					Categoria = p.Categoria,
					Descripcion = p.Descripcion,
					TiempoPreparacion = p.TiempoPreparacion,
					ImagenProducto = p.ImagenProducto,
					Activo = p.Activo
				})
				.ToList();

			return Ok(productos);
		}

		[HttpGet("Categorias")]
		public IActionResult ObtenerCategorias()
		{
			var categorias = _repository.GetAll()
				.Where(p => !string.IsNullOrEmpty(p.Categoria))
				.Select(p => p.Categoria)
				.Distinct()
				.OrderBy(c => c)
				.ToList();

			return Ok(categorias);
		}
	}
}