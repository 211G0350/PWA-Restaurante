namespace PWA_Restaurante.Models.DTOs
{
	public class PedidoDetalleDTO
	{
		public int Id { get; set; }
		public string NombreProducto { get; set; } = string.Empty;
		public int Cantidad { get; set; }
		public decimal PrecioUnitario { get; set; }
		public decimal Subtotal { get; set; }

	}
}
