namespace PWA_Restaurante.Models.DTOs
{
	public class PedidoResumen_DTO
	{
		public int Id { get; set; }
		public int NumMesa { get; set; }
		public DateTime TomadoEn { get; set; }
		public decimal PrecioTotal { get; set; }
		public string Estado { get; set; } = string.Empty;

	}
}
