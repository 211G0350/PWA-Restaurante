using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;

namespace PWA_Restaurante.Models.Validators
{
	public class ProductosValidator
	{
		private readonly Repository<Productos> repository;

		public ProductosValidator(Repository<Productos> repository)
		{
			this.repository = repository;
		}

		public bool ValidateAgregar(AgregarProductoDTO producto, out List<string> errores)
		{
			errores = new List<string>();

			if (string.IsNullOrWhiteSpace(producto.Nombre))
			{
				errores.Add("El nombre del producto es requerido.");
			}

			if (string.IsNullOrWhiteSpace(producto.Categoria))
			{
				errores.Add("La categoría es requerida.");
			}

			if (producto.Precio <= 0)
			{
				errores.Add("El precio debe ser mayor a 0.");
			}

			if (producto.TiempoPreparacion < 0)
			{
				errores.Add("El tiempo de preparación no puede ser negativo.");
			}

			if (producto.Nombre.Length > 90)
			{
				errores.Add("El nombre del producto debe tener una longitud máxima de 90 caracteres.");
			}

			if (producto.Categoria.Length > 25)
			{
				errores.Add("La categoría debe tener una longitud máxima de 25 caracteres.");
			}

			if (!string.IsNullOrWhiteSpace(producto.Descripcion) && producto.Descripcion.Length > 1000)
			{
				errores.Add("La descripción debe tener una longitud máxima de 1000 caracteres.");
			}

			if (!string.IsNullOrWhiteSpace(producto.ImagenProducto) && producto.ImagenProducto.Length > 2048)
			{
				errores.Add("La URL de la imagen debe tener una longitud máxima de 2048 caracteres.");
			}

			// Verificar si ya existe un producto con el mismo nombre
			if (!string.IsNullOrWhiteSpace(producto.Nombre) &&
				repository.GetAll().Any(x => x.Nombre.ToLower() == producto.Nombre.ToLower()))
			{
				errores.Add("Ya existe un producto con el mismo nombre.");
			}

			return errores.Count == 0;
		}

		public bool ValidateEditar(EditarProductoDTO producto, out List<string> errores)
		{
			errores = new List<string>();

			if (string.IsNullOrWhiteSpace(producto.Nombre))
			{
				errores.Add("El nombre del producto es requerido.");
			}

			if (string.IsNullOrWhiteSpace(producto.Categoria))
			{
				errores.Add("La categoría es requerida.");
			}

			if (producto.Precio <= 0)
			{
				errores.Add("El precio debe ser mayor a 0.");
			}

			if (producto.TiempoPreparacion < 0)
			{
				errores.Add("El tiempo de preparación no puede ser negativo.");
			}

			if (producto.Nombre.Length > 90)
			{
				errores.Add("El nombre del producto debe tener una longitud máxima de 90 caracteres.");
			}

			if (producto.Categoria.Length > 25)
			{
				errores.Add("La categoría debe tener una longitud máxima de 25 caracteres.");
			}

			if (!string.IsNullOrWhiteSpace(producto.Descripcion) && producto.Descripcion.Length > 1000)
			{
				errores.Add("La descripción debe tener una longitud máxima de 1000 caracteres.");
			}

			if (!string.IsNullOrWhiteSpace(producto.ImagenProducto) && producto.ImagenProducto.Length > 2048)
			{
				errores.Add("La URL de la imagen debe tener una longitud máxima de 2048 caracteres.");
			}

			// Verificar si ya existe otro producto con el mismo nombre (excluyendo el producto actual)
			if (!string.IsNullOrWhiteSpace(producto.Nombre) &&
				repository.GetAll().Any(x => x.Nombre.ToLower() == producto.Nombre.ToLower() && x.Id != producto.Id))
			{
				errores.Add("Ya existe otro producto con el mismo nombre.");
			}

			// Verificar que el producto existe
			if (!repository.GetAll().Any(x => x.Id == producto.Id))
			{
				errores.Add("El producto no existe.");
			}

			return errores.Count == 0;
		}

		public bool ValidateEliminar(int id, out List<string> errores)
		{
			errores = new List<string>();

			var producto = repository.GetAll().FirstOrDefault(x => x.Id == id);
			if (producto == null)
			{
				errores.Add("El producto no existe.");
				return false;
			}

			// Verificar si el producto tiene pedidos asociados
			if (producto.PedidoDetalles.Any())
			{
				errores.Add("No se puede eliminar el producto porque tiene pedidos asociados.");
			}

			return errores.Count == 0;
		}
	}
}
