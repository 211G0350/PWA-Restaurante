namespace PWA_Restaurante.Models.DTOs
{
	public class PedidoDetalleCompletoDTO
	{
		public int Id { get; set; }
		public int NumMesa { get; set; }
		public string? NotasEspeciales { get; set; }
		public string UsuarioNombre { get; set; } = string.Empty;
		public DateTime TomadoEn { get; set; }
		public string Estado { get; set; } = string.Empty;
		public decimal PrecioTotal { get; set; }
		public List<PedidoDetalleDTO> Detalles { get; set; } = new List<PedidoDetalleDTO>();

	}
}
