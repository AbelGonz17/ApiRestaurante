using RestauranteAPI.Entidades;

namespace RestauranteAPI.DTOs
{
    public class PedidoDTO
    {
        public int Id { get; set; }
        public required Guid ClienteId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public EstadoPedido Estado { get; set; }
        public decimal Total { get; set; }
        public Cliente Cliente { get; set; }
        public List<PedidoDetalleDTO> pedidoDetalles { get; set; } = new List<PedidoDetalleDTO>();
    }
}
