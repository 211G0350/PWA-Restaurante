using PWA_Restaurante.Models.DTOs;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Repositories;
using System.Text.RegularExpressions;

namespace PWA_Restaurante.Models.Validators
{
	public class UsuarioValidator
	{
		private readonly Repository<Usuarios> repository;

		public UsuarioValidator(Repository<Usuarios> repository)
		{
			this.repository = repository;
		}

		public bool Validate(UsuarioDTO user, out List<string> errores)
		{
			errores = new List<string>();

			if (string.IsNullOrWhiteSpace(user.Nombre))
			{
				errores.Add("El nombre de usuario está vacío.");
			}

			if (string.IsNullOrWhiteSpace(user.Correo))
			{
				errores.Add("El correo está vacío.");
			}

			if (string.IsNullOrWhiteSpace(user.Contrasena))
			{
				errores.Add("La contraseña está vacía.");
			}

			if (string.IsNullOrWhiteSpace(user.Rol))
			{
				errores.Add("El rol está vacío.");
			}

			if (user.Nombre.Length > 100)
			{
				errores.Add("El nombre de usuario debe tener una longitud máxima de 100 caracteres.");
			}

			if (user.Correo.Length > 255)
			{
				errores.Add("El correo debe tener una longitud máxima de 255 caracteres.");
			}

			if (user.Contrasena.Length > 255)
			{
				errores.Add("La contraseña debe tener una longitud máxima de 255 caracteres.");
			}

			if (user.Rol.Length > 50)
			{
				errores.Add("El rol debe tener una longitud máxima de 50 caracteres.");
			}

			// Validar formato de correo
			if (!string.IsNullOrWhiteSpace(user.Correo) && !Regex.IsMatch(user.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
			{
				errores.Add("El formato del correo electrónico no es válido.");
			}

			// Verificar si ya existe un usuario con el mismo correo
			if (repository.GetAll().Any(x => x.Correo == user.Correo))
			{
				errores.Add("Ya existe un usuario con el mismo correo.");
			}

			// Validar contraseña (al menos 6 caracteres, una mayúscula y una minúscula)
			if (!string.IsNullOrWhiteSpace(user.Contrasena) && !Regex.IsMatch(user.Contrasena, @"^(?=.*[A-ZÑ])(?=.*[a-zñ]).{6,}$"))
			{
				errores.Add("La contraseña debe tener al menos 6 caracteres, una letra mayúscula y una letra minúscula.");
			}

			return errores.Count == 0;
		}


		public bool ValidateEdicion(EditarUsuarioDTO user, out List<string> errores)
		{
			errores = new List<string>();

			if (string.IsNullOrWhiteSpace(user.Nombre))
			{
				errores.Add("El nombre de usuario está vacío.");
			}

			if (string.IsNullOrWhiteSpace(user.Correo))
			{
				errores.Add("El correo está vacío.");
			}

			if (string.IsNullOrWhiteSpace(user.Rol))
			{
				errores.Add("El rol está vacío.");
			}

			if (user.Nombre.Length > 100)
			{
				errores.Add("El nombre de usuario debe tener una longitud máxima de 100 caracteres.");
			}

			if (user.Correo.Length > 255)
			{
				errores.Add("El correo debe tener una longitud máxima de 255 caracteres.");
			}

			if (user.Rol.Length > 50)
			{
				errores.Add("El rol debe tener una longitud máxima de 50 caracteres.");
			}

			// Validar formato de correo
			if (!string.IsNullOrWhiteSpace(user.Correo) && !Regex.IsMatch(user.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
			{
				errores.Add("El formato del correo electrónico no es válido.");
			}

			// Verificar si ya existe otro usuario con el mismo correo (excluyendo el usuario actual)
			if (repository.GetAll().Any(x => x.Correo == user.Correo && x.Id != user.Id))
			{
				errores.Add("Ya existe otro usuario con el mismo correo.");
			}

			// Validar contraseña solo si se proporciona
			if (!string.IsNullOrWhiteSpace(user.Contrasena))
			{
				if (user.Contrasena.Length > 255)
				{
					errores.Add("La contraseña debe tener una longitud máxima de 255 caracteres.");
				}

				if (!Regex.IsMatch(user.Contrasena, @"^(?=.*[A-ZÑ])(?=.*[a-zñ]).{6,}$"))
				{
					errores.Add("La contraseña debe tener al menos 6 caracteres, una letra mayúscula y una letra minúscula.");
				}
			}

			return errores.Count == 0;
		}



	}
	}
