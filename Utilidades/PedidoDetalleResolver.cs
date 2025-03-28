using AutoMapper;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;

namespace RestauranteAPI.Utilidades
{
    public class PedidoDetalleResolver : IValueResolver<CreacionPedidoDTO, Pedido, List<PedidoDetalle>>
    {
        private readonly ApplicationDbContext _context;

        public PedidoDetalleResolver(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PedidoDetalle> Resolve(CreacionPedidoDTO source, Pedido destination, List<PedidoDetalle> destMember, ResolutionContext context)
        {
            var resultado = new List<PedidoDetalle>();

            if (source.Platos == null) return resultado;

            // Obtener platos disponibles desde la base de datos
            var platosDisponibles = _context.Platos.ToList();

            foreach (var platoPedido in source.Platos)
            {
                var plato = platosDisponibles.FirstOrDefault(p => p.Id == platoPedido.PlatoId);

                if (plato != null)
                {
                    resultado.Add(new PedidoDetalle
                    {
                        PedidoId = destination.Id,
                        pedido = destination,
                        PlatoId = platoPedido.PlatoId,
                        Cantidad = platoPedido.Cantidad,
                        precioUnitario = plato.precio
                    });
                }
            }

            return resultado;
        }
    }
}
