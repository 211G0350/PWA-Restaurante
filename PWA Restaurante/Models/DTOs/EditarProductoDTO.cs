namespace PWA_Restaurante.Models.DTOs
{
	public class EditarProductoDTO
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public decimal Precio { get; set; }
		public string Categoria { get; set; } = string.Empty;
		public string? Descripcion { get; set; }
		public int TiempoPreparacion { get; set; }
		public string? ImagenProducto { get; set; }
		public bool Activo { get; set; }

	}
}
