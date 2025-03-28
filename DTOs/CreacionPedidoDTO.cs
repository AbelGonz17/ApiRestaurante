using RestauranteAPI.Entidades;

namespace RestauranteAPI.DTOs
{
    public class CreacionPedidoDTO
    {   
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public List<PlatoPedidoDTO> Platos { get; set; }
      
    }
}
