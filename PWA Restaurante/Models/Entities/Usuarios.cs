using System;
using System.Collections.Generic;

namespace PWA_Restaurante.Models.Entities;

public partial class Usuarios
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public virtual ICollection<Pedidos> Pedidos { get; set; } = new List<Pedidos>();
}
