namespace PWA_Restaurante.Models.DTOs
{
	public class PedidoDetalleCrearDTO
	{
		public int ProductoId { get; set; }
		public int Cantidad { get; set; }
		public decimal PrecioUnitario { get; set; }

	}
}
