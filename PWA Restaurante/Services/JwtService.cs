using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

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

		private string HashPassword(string password)
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}


		public string? GenerarToken(LoginDTO dto)
		{
			var usuario = Repository.GetAll()
				.FirstOrDefault(x => x.Correo == dto.Correo);

			if (usuario == null)
			{
				return null;
			}

			string contrasenaEncriptada = HashPassword(dto.Contrasena);
			if (usuario.Contrasena != contrasenaEncriptada)
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
