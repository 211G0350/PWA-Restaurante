namespace PWA_Restaurante.Models.DTOs
{
	public class EditarPedidoDTO
	{
		public int Id { get; set; }
		public int NumMesa { get; set; }
		public string? NotasEspeciales { get; set; }
		public int UsuarioId { get; set; }
		public string Estado { get; set; } = string.Empty;
		public List<PedidoDetalleCrearDTO> Detalles { get; set; } = new List<PedidoDetalleCrearDTO>();

	}
}
