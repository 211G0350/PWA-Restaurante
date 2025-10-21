using System;
using System.Collections.Generic;

namespace PWA_Restaurante.Models.Entities;

public partial class Productos
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal Precio { get; set; }

    public string Categoria { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int TiempoPreparacion { get; set; }

    public string? ImagenProducto { get; set; }

    public bool? Activo { get; set; }

    public virtual ICollection<PedidoDetalles> PedidoDetalles { get; set; } = new List<PedidoDetalles>();
}
