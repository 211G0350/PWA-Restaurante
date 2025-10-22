namespace PWA_Restaurante.Models.DTOs
{
	public class AgregarPedidoDTO
	{
		public int NumMesa { get; set; }
		public string? NotasEspeciales { get; set; }
		public int UsuarioId { get; set; }
		public string Estado { get; set; } = "Pendiente";
		public List<PedidoDetalleCrearDTO> Detalles { get; set; } = new List<PedidoDetalleCrearDTO>();

	}
}
