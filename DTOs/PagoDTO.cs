using RestauranteAPI.Entidades;

namespace RestauranteAPI.DTOs
{
    public class PagoDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public MetodoDePago MetodoDePago { get; set; }
        public Pedido Pedido { get; set; }
    }
}
