namespace RestauranteAPI.Entidades
{
    public class Pago
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public MetodoDePago MetodoDePago { get; set; }
        public Pedido Pedido { get; set; }  
    }

    public enum MetodoDePago
    {
        Tarjeta = 1,
        Efectivo = 2
    }
}
