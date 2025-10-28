using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;
using PWA_Restaurante.Models.Validators;

namespace PWA_Restaurante.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductosController : ControllerBase
	{
		private readonly Repository<Productos> _repository;
		private readonly ProductosValidator _validator;

		public ProductosController(Repository<Productos> repository, ProductosValidator validator)
		{
			_repository = repository;
			_validator = validator;
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
		//[Authorize(Roles = "Admin")]
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
		//[Authorize(Roles = "Admin")]
		public IActionResult AgregarProducto(AgregarProductoDTO dto)
		{
			if (_validator.ValidateAgregar(dto, out List<string> errores))
			{
			// si se indica una categoria que no existe, se agregará esa nueva categoria tmb


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
				
				var mensaje = categoriaExistente 
					? "Producto agregado exitosamente" 
					: $"Producto agregado exitosamente. Nueva categoría '{dto.Categoria}' creada automáticamente";
				
				return Ok(new { message = mensaje, id = producto.Id });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[HttpPut("Editar")]
		//[Authorize(Roles = "Admin")]
		public IActionResult EditarProducto(EditarProductoDTO dto)
		{
			if (_validator.ValidateEditar(dto, out List<string> errores))
			{
				var productoExistente = _repository.GetByIdWithTracking(dto.Id);

				productoExistente.Nombre = dto.Nombre;
				productoExistente.Precio = dto.Precio;
				productoExistente.Categoria = dto.Categoria;
				productoExistente.Descripcion = dto.Descripcion;
				productoExistente.TiempoPreparacion = dto.TiempoPreparacion;
				productoExistente.ImagenProducto = dto.ImagenProducto;
				productoExistente.Activo = dto.Activo;

				_repository.Update(productoExistente);
				return Ok(new { message = "Producto actualizado exitosamente" });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[HttpDelete("EliminarProducto/{id}")]
		//[Authorize(Roles = "Admin")]
		public IActionResult EliminarProducto(int id)
		{
			if (_validator.ValidateEliminar(id, out List<string> errores))
			{
				var producto = _repository.GetByIdWithTracking(id);
				_repository.Delete(producto.Id);
				return Ok(new { message = "Producto eliminado exitosamente" });
			}
			else
			{
				return BadRequest(errores);
			}
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
	}
}