namespace PWA_Restaurante.Models.DTOs
{
	public class EditarUsuarioDTO
	{
		public int Id { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Rol { get; set; } = string.Empty;
		public string Correo { get; set; } = string.Empty;
		public string? Contrasena { get; set; }



	}
}
