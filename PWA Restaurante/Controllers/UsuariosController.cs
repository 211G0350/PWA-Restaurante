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


		[Authorize]
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
					Contrasena = dto.Contrasena
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
			var token = Service.GenerarToken(dto);
			if (token == null)
			{
				return Unauthorized("Correo o contraseña incorrectos");
			}

			var usuario = Service.Repository.GetAll()
						  .FirstOrDefault(x => x.Correo == dto.Correo);

			return Ok(new
			{
				Token = token,
				Rol = usuario?.Rol,
				Usuario = usuario?.Nombre,
				Id = usuario?.Id
			});
		}

		[Authorize]
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

		[Authorize]
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

		[Authorize]
		[HttpPut("Editar")]
		public IActionResult Editar(EditarUsuarioDTO dto)
		{
			var usuarioExistente = Repository.GetAll().FirstOrDefault(x => x.Id == dto.Id);
			if (usuarioExistente == null)
			{
				return NotFound("Usuario no encontrado");
			}

			if (Validator.ValidateEdicion(dto, out List<string> errores))
			{
				usuarioExistente.Nombre = dto.Nombre;
				usuarioExistente.Rol = dto.Rol;
				usuarioExistente.Correo = dto.Correo;

				// actualiz contraseña
				if (!string.IsNullOrWhiteSpace(dto.Contrasena))
				{
					usuarioExistente.Contrasena = dto.Contrasena;
				}

				Repository.Update(usuarioExistente);
				return Ok(new { message = "Usuario actualizado exitosamente" });
			}
			else
			{
				return BadRequest(errores);
			}
		}

		[Authorize]
		[HttpDelete("Eliminar/{id}")]
		public IActionResult Eliminar(int id)
		{
			var usuario = Repository.GetAll().FirstOrDefault(x => x.Id == id);
			if (usuario == null)
			{
				return NotFound("Usuario no encontrado");
			}

			// Verificar si el usuario tiene pedidos asociados
			// 
			// var tienePedidos = Repository.GetAll().Any(x => x.Pedidos.Any());
			// if (tienePedidos)
			// {
			//     return BadRequest("No se puede eliminar el usuario porque tiene pedidos asociados");
			// }

			Repository.Delete(usuario);
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
