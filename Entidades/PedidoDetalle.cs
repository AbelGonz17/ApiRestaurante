namespace RestauranteAPI.Entidades
{
    //Un Pedido puede tener varios Platos.
   // Un Plato puede estar en varios Pedidos.
    public class PedidoDetalle
    {
       public int PedidoId { get; set; }
        public Pedido pedido { get; set; }
        public int PlatoId { get; set; }
        public Plato Plato { get; set; }
        public int Cantidad { get; set; }
        public decimal precioUnitario { get; set; }
    }
}
