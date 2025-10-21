using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace PWA_Restaurante.Models.Entities;

public partial class RestauranteContext : DbContext
{
    public RestauranteContext()
    {
    }

    public RestauranteContext(DbContextOptions<RestauranteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PedidoDetalles> PedidoDetalles { get; set; }

    public virtual DbSet<Pedidos> Pedidos { get; set; }

    public virtual DbSet<Productos> Productos { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("user=root;password=root;server=localhost;database=restaurante", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.34-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<PedidoDetalles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("pedido_detalles");

            entity.HasIndex(e => e.PedidoId, "fk_detalles_pedido");

            entity.HasIndex(e => e.ProductoId, "fk_detalles_producto");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.PedidoId).HasColumnName("pedido_id");
            entity.Property(e => e.PrecioUnitario)
                .HasPrecision(10, 2)
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");
            entity.Property(e => e.Subtotal)
                .HasPrecision(12, 2)
                .HasComputedColumnSql("`cantidad` * `precio_unitario`", true)
                .HasColumnName("subtotal");

            entity.HasOne(d => d.Pedido).WithMany(p => p.PedidoDetalles)
                .HasForeignKey(d => d.PedidoId)
                .HasConstraintName("fk_detalles_pedido");

            entity.HasOne(d => d.Producto).WithMany(p => p.PedidoDetalles)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_detalles_producto");
        });

        modelBuilder.Entity<Pedidos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("pedidos");

            entity.HasIndex(e => e.UsuarioId, "fk_pedidos_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'pendiente'")
                .HasColumnType("enum('pendiente','enviado','en preparacion','listo')")
                .HasColumnName("estado");
            entity.Property(e => e.NotasEspeciales)
                .HasColumnType("text")
                .HasColumnName("notas_especiales");
            entity.Property(e => e.NumMesa).HasColumnName("num_mesa");
            entity.Property(e => e.PrecioTotal)
                .HasPrecision(12, 2)
                .HasColumnName("precio_total");
            entity.Property(e => e.TomadoEn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("tomado_en");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pedidos_usuario");
        });

        modelBuilder.Entity<Productos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("productos");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("activo");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .HasColumnName("categoria");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.ImagenProducto)
                .HasMaxLength(2048)
                .HasColumnName("imagen_producto");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasPrecision(10, 2)
                .HasColumnName("precio");
            entity.Property(e => e.TiempoPreparacion).HasColumnName("tiempo_preparacion");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "correo").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo).HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
