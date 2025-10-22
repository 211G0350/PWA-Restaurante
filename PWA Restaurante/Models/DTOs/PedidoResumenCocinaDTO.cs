namespace PWA_Restaurante.Models.DTOs
{
	public class PedidoResumenCocinaDTO
	{
		public int Id { get; set; }
		public int NumMesa { get; set; }
		public DateTime TomadoEn { get; set; }
		public decimal PrecioTotal { get; set; }
		public string Estado { get; set; } = string.Empty;
		public string UsuarioNombre { get; set; } = string.Empty;

	}
}
