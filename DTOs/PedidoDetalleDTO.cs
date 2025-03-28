namespace RestauranteAPI.DTOs
{
    public class PedidoDetalleDTO
    {
        public int PedidoId { get; set; }
        public int PlatoId { get; set; }
        public string PlatoNombre { get; set; }
        public decimal PlatoPrecio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

    }
}
