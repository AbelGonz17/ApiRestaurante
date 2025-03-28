using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestauranteAPI.Entidades;
using System.Numerics;
using System.Reflection.Emit;

namespace RestauranteAPI
{
    public class ApplicationDbContext :IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Plato>()
               .Property(p => p.precio)
               .HasColumnType("decimal(18,2)");

            builder.Entity<Pedido>()
          .Property(p => p.Total)
          .HasColumnType("decimal(18,2)");

            builder.Entity<PedidoDetalle>()
                .HasKey(al => new { al.PedidoId, al.PlatoId });

            builder.Entity<PedidoDetalle>()
          .Property(p => p.precioUnitario)
          .HasColumnType("decimal(18,2)");

            builder.Entity<Pago>()
        .Property(p => p.Monto)
        .HasColumnType("decimal(18,2)");

        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Plato> Platos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<Pago> Pagos { get; set; }
    }
}
