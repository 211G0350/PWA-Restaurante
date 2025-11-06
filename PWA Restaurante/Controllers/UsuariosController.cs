using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Models.Validators;
using PWA_Restaurante.Repositories;
using PWA_Restaurante.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PWA_Restaurante.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UsuariosController : ControllerBase
	{
		public UsuariosController(Repository<Usuarios> repository, UsuarioValidator validator, JwtService service)
		{
			Repository = repository;
			Validator = validator;
			Service = service;
		}

		public Repository<Usuarios> Repository { get; }
		public UsuarioValidator Validator { get; }
		public JwtService Service { get; }

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


		[Authorize(Roles = "Admin")]
		[HttpPost("Registrar")]
		public IActionResult Registrar(UsuarioDTO dto)
		{
			if (Validator.Validate(dto, out List<string> errores))
			{
				Usuarios user = new()
				{
					Nombre = dto.Nombre,
					Rol = dto.Rol,
					Correo = dto.Correo,
					Contrasena = HashPassword(dto.Contrasena)
				};
				Repository.Insert(user);
				return Ok(new { message = "Usuario registrado exitosamente" });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[AllowAnonymous]
		[HttpPost("Login")]
		public IActionResult Login(LoginDTO dto)
		{
			var usuario = Service.Repository.GetAll()
						  .FirstOrDefault(x => x.Correo == dto.Correo);

			if (usuario == null)
			{
				return NotFound(new { message = "No existe un usuario con ese correo" });
			}

			var token = Service.GenerarToken(dto);
			if (token == null)
			{
				return Unauthorized(new { message = "Contraseña incorrecta" });
			}

			return Ok(new
			{
				Token = token,
				Rol = usuario.Rol,
				Usuario = usuario.Nombre,
				Id = usuario.Id
			});
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("ObtenerTodos")]
		public IActionResult ObtenerTodos()
		{
			var usuarios = Repository.GetAll()
				.Select(u => new UsuarioRespuestaDTO
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Rol = u.Rol,
					Correo = u.Correo
				})
				.ToList();

			return Ok(usuarios);
		}

		[Authorize(Roles = "Admin")]
		[HttpGet("ObtenerPorId/{id}")]
		public IActionResult ObtenerPorId(int id)
		{
			var usuario = Repository.GetAll().FirstOrDefault(x => x.Id == id);
			if (usuario == null)
			{
				return NotFound("Usuario no encontrado");
			}

			var usuarioRespuesta = new UsuarioRespuestaDTO
			{
				Id = usuario.Id,
				Nombre = usuario.Nombre,
				Rol = usuario.Rol,
				Correo = usuario.Correo
			};

			return Ok(usuarioRespuesta);
		}

		[Authorize(Roles = "Admin")]
		[HttpPut("Editar/{id}")]
		public IActionResult Editar(int id, EditarUsuarioDTO dto)
		{
			var usuarioExistente = Repository.GetAll().FirstOrDefault(x => x.Id == id);
			if (usuarioExistente == null)
			{
				return NotFound("Usuario no encontrado");
			}

			dto.Id = id;

			if (Validator.ValidateEdicion(dto, out List<string> errores))
			{
				usuarioExistente.Nombre = dto.Nombre;
				usuarioExistente.Rol = dto.Rol;
				usuarioExistente.Correo = dto.Correo;

			if (!string.IsNullOrWhiteSpace(dto.Contrasena))
			{
				usuarioExistente.Contrasena = HashPassword(dto.Contrasena);
			}

				Repository.Update(usuarioExistente);
				return Ok(new { message = "Usuario actualizado exitosamente" });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete("Eliminar/{id}")]
		public IActionResult Eliminar(int id)
		{
			var usuario = Repository.GetAll().FirstOrDefault(x => x.Id == id);
			if (usuario == null)
			{
				return NotFound("Usuario no encontrado");
			}

			// Verificar si el usuario tiene pedidos asociados
			var tienePedidos = Repository.GetAll().Any(x => x.Pedidos.Any());
			 if (tienePedidos)
			 {
		     return BadRequest("No se puede eliminar el usuario porque tiene pedidos asociados");
			 }

			Repository.Delete(usuario.Id);
			return Ok(new { message = "Usuario eliminado exitosamente" });
		}


		[Authorize]
		[HttpGet("Perfil")]
		public IActionResult ObtenerPerfil()
		{
			var userIdClaim = User.FindFirst("IdUsuario")?.Value;
			if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
			{
				return Unauthorized("Token inválido");
			}

			var usuario = Repository.GetAll().FirstOrDefault(x => x.Id == userId);
			if (usuario == null)
			{
				return NotFound("Usuario no encontrado");
			}

			var usuarioRespuesta = new UsuarioRespuestaDTO
			{
				Id = usuario.Id,
				Nombre = usuario.Nombre,
				Rol = usuario.Rol,
				Correo = usuario.Correo
			};

			return Ok(usuarioRespuesta);
		}



	}
}
