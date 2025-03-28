namespace RestauranteAPI.Entidades
{
    public class Pedido
    {
        public int Id { get; set; }
        public required Guid ClienteId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public EstadoPedido Estado { get; set; }
        public decimal Total { get; set; }
        public Cliente Cliente { get; set; }
        public List<PedidoDetalle> pedidoDetalles { get; set; } = new List<PedidoDetalle>();
    }

    public enum EstadoPedido
    {
        Pendiente = 1,
        EnProceso= 2,
        Enviado = 3,
        Entregado = 4
    }
}
