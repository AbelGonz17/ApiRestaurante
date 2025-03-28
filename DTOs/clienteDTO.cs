namespace RestauranteAPI.DTOs
{
    public class clienteDTO
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
