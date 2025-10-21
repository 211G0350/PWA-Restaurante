using System;
using System.Collections.Generic;

namespace PWA_Restaurante.Models.Entities;

public partial class PedidoDetalles
{
    public ulong Id { get; set; }

    public ulong PedidoId { get; set; }

    public ulong ProductoId { get; set; }

    public uint Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal? Subtotal { get; set; }

    public virtual Pedidos Pedido { get; set; } = null!;

    public virtual Productos Producto { get; set; } = null!;
}
