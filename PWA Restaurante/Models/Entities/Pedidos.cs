using System;
using System.Collections.Generic;

namespace PWA_Restaurante.Models.Entities;

public partial class Pedidos
{
    public int Id { get; set; }

    public int NumMesa { get; set; }

    public string? NotasEspeciales { get; set; }

    public int UsuarioId { get; set; }

    public DateTime TomadoEn { get; set; }

    public string Estado { get; set; } = null!;

    public decimal PrecioTotal { get; set; }

    public virtual ICollection<PedidoDetalles> PedidoDetalles { get; set; } = new List<PedidoDetalles>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
