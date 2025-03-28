namespace RestauranteAPI.Entidades
{
    public class Cliente
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
