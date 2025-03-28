using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;
using System.Security.Claims;

namespace RestauranteAPI.Controllers
{
    [ApiController]
    [Route("api/pago")]
    public class PagoController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;

        public PagoController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        public async Task<ActionResult<List<PagoDTO>>>Get()
        {
            var pago = await context.Pagos
                .Include(x => x.Pedido)
                .ToListAsync();

            return mapper.Map<List<PagoDTO>>(pago);
        }

        [HttpGet("{id:int}", Name = "obtenerPorId")]
        public async Task<ActionResult<PagoDTO>> GetById(int id)
        {
            var existePago = await context.Pagos
                .Include(x => x.Pedido)
                .AnyAsync(x => x.Id == id);
            if(!existePago)
            {
                return NotFound($"no se encuntra el pago con el id {id}");
            }

            var pagoDTO = mapper.Map<PagoDTO>(existePago);
            return pagoDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreacionPagoDTO creacionPagoDTO)
        {
            var clienteId = User.FindFirstValue("UsuarioId");

            if (string.IsNullOrEmpty(clienteId))
            {
                return BadRequest("usario no autenticado");
            }

            var pedido = await context.Pedidos
                .Where(x => x.ClienteId.ToString() == clienteId && x.Estado == EstadoPedido.Enviado)
                .FirstOrDefaultAsync();

            if (pedido == null)
            {
                return NotFound("No se encontró un pedido en estado 'Enviado' para este cliente.");
            }

            var pagoExistente = await context.Pagos.AnyAsync(x => x.PedidoId == pedido.Id);
            if (pagoExistente)
            {
                return BadRequest("Este pedido ya ha sido pagado anteriormente.");
            }

            var pago = mapper.Map<Pago>(creacionPagoDTO, opt => opt.Items["Pedido"] = pedido);

            context.Add(pago);

            pedido.Estado = EstadoPedido.Entregado;

            await context.SaveChangesAsync();

            return Ok("Pago realizado. ¡Gracias por contar con nosotros!");
        }
    }
}
