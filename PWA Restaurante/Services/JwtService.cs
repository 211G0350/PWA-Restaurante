using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace PWA_Restaurante.Services
{
	public class JwtService
	{
		public JwtService(IConfiguration configuration, Repository<Usuarios> repository)
		{
			Configuration = configuration;
			Repository = repository;
		}
		public IConfiguration Configuration { get; }
		public Repository<Usuarios> Repository { get; }

		public string? GenerarToken(LoginDTO dto)
		{
			var usuario = Repository.GetAll()
				.FirstOrDefault(x => x.Correo == dto.Correo && x.Contrasena == dto.Contrasena);

			if (usuario == null)
			{
				return null;
			}

			var claims = new List<Claim>
			{
				new Claim("IdUsuario", usuario.Id.ToString()),
				new Claim(ClaimTypes.Name, usuario.Nombre),
				new Claim(ClaimTypes.Email, usuario.Correo),
				new Claim(ClaimTypes.Role, usuario.Rol)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var descriptor = new JwtSecurityToken(
				issuer: Configuration["Jwt:Issuer"],
				audience: Configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(60),
				signingCredentials: creds
			);

			var handler = new JwtSecurityTokenHandler();
			return handler.WriteToken(descriptor);
		}

		public Usuarios? ObtenerUsuarioDesdeToken(string token)
		{
			try
			{
				var handler = new JwtSecurityTokenHandler();
				var jsonToken = handler.ReadJwtToken(token);

				var idClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "IdUsuario");
				if (idClaim != null && int.TryParse(idClaim.Value, out int id))
				{
					return Repository.GetAll().FirstOrDefault(x => x.Id == id);
				}
			}
			catch
			{
				return null;
			}

			return null;
		}




	}
}
