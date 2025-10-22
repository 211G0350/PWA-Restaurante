namespace PWA_Restaurante.Models.DTOs
{
	public class CancelarPedidoDTO
	{
		public int Id { get; set; }
		public int NumMesa { get; set; }
		public DateTime TomadoEn { get; set; }
		public decimal PrecioTotal { get; set; }

	}
}
