namespace PWA_Restaurante.Models.DTOs
{
	public class ActualizarEstadoDTO
	{
		public int Id { get; set; }
		public string EstadoActual { get; set; } = string.Empty;
		public string NuevoEstado { get; set; } = string.Empty;

	}
}
