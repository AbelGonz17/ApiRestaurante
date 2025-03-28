using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;


namespace RestauranteAPI.Controllers
{
    [ApiController]
    [Route("api/pedidos")]
    [Authorize]
    public class PedidoController : ControllerBase
    {

        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;

        private readonly ApplicationDbContext context;

        public PedidoController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;

        }

        [HttpGet]
        public async Task<ActionResult<List<PedidoDTO>>> Get()
        {
            var pedidos = await context.Pedidos
                .OrderBy(x => x.Fecha)
                .ProjectTo<PedidoDTO>(mapper.ConfigurationProvider)
                .ToListAsync();
            return pedidos;
        }

        [HttpGet("{id:int}", Name = "ObtenerPedidoId")]
        public async Task<ActionResult<PedidoDTO>> GetId(int id)
        {
            var pedido = await context.Pedidos
                .Include(x => x.pedidoDetalles)
                .ThenInclude(x => x.Plato)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pedido == null)
            {
                return NotFound("No existe el pedido con el id ingresado");
            }

            return mapper.Map<PedidoDTO>(pedido);
        }

        [HttpGet("{Id:guid}", Name = "obtenerPorCliente")]
        public async Task<ActionResult<List<PedidoDTO>>> GetByCliente(Guid id)
        {
            var pedidos = await context.Pedidos
                .Where(x => x.ClienteId == id) 
                .Include(x => x.pedidoDetalles) 
                .ThenInclude(x => x.Plato) 
                .ToListAsync();

            if (pedidos == null)
            {

                return NotFound(new { mensaje = "El cliente registrado no tiene pedidos." });
            }

            var pedidoDTO = mapper.Map<List<PedidoDTO>>(pedidos);
            return Ok(pedidoDTO);
        }

        [HttpPut("EnviarPedido/{pedidoId}")]
        public async Task<ActionResult> EnviarPedido(int pedidoId)
        {
            var pedido = await context.Pedidos.FindAsync(pedidoId);

            if(pedido == null)
            {
                return NotFound($"el pedido con el id {pedidoId} no se encuentra en el sistema");
            }

            if (pedido.Estado != EstadoPedido.EnProceso)
            {
                return BadRequest("el pedido aun no esta en proceso , no puede ser enviado");
            }

            pedido.Estado = EstadoPedido.Enviado;

            var cambios = await context.SaveChangesAsync();

            //savechanges devuelve un entero (el numero de filas que se cambio)
            if (cambios > 0)
            {
                return Ok("El pedido fue enviado exitosamente.");
            }

            return StatusCode(500, "Ocurrió un error al actualizar el pedido.");
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]CreacionPedidoDTO creacionPedidoDTO)
        {
            var usuarioId = User.FindFirstValue("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
            {
                return BadRequest("Usuario no autenticado");
            }

            if (!Guid.TryParse(usuarioId, out Guid clienteGuid))
            {
                return BadRequest("El ID del usuario no tiene el formato correcto.");
            }

            var ExisteUser = await context.Clientes
               .AnyAsync(x => x.Id == clienteGuid);
            if (!ExisteUser)
            {
                return BadRequest("El usuario no se encuentra en el sistema");
            }

            var pedidoExistente = await context.Pedidos
                  .Include(p => p.pedidoDetalles)
                  .ThenInclude(d => d.Plato)
                  .FirstOrDefaultAsync(x => x.ClienteId == clienteGuid && x.Estado != EstadoPedido.Entregado);

            if (pedidoExistente != null)
            {
                var pedidoDtoE = mapper.Map<PedidoDTO>(pedidoExistente);
                return BadRequest(new
                {
                    mensaje = "Ya tienes un pedido en curso. No puedes crear otro hasta completarlo.",
                    pedidoPendiente = pedidoDtoE
                });
            }

            if (creacionPedidoDTO.Platos is null || creacionPedidoDTO.Platos.Count == 0)
            {
                ModelState.AddModelError(nameof(creacionPedidoDTO.Platos), 
                    "no se puede crear un pedido sin platos");
                return ValidationProblem();

            }

            var platosIds = creacionPedidoDTO.Platos.Select(x => x.PlatoId).ToList();

            var platosDisponibles = await context.Platos
                .Where(x => platosIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.precio);
                

            if(platosIds.Count != platosDisponibles.Count)
            {
                var platosNoExisten = platosIds.Except(platosDisponibles.Keys);
                var platosNoExistenString = string.Join(",", platosNoExisten);
                ModelState.AddModelError(nameof(creacionPedidoDTO.Platos),
                    $"Los siguientes platos no existen: {platosNoExistenString}");
                return ValidationProblem();
            }

            int pedidosEnProceso = await context.Pedidos.CountAsync(p => p.Estado == EstadoPedido.EnProceso);

            EstadoPedido estadoPedido = pedidosEnProceso >= 3 ? EstadoPedido.Pendiente : EstadoPedido.EnProceso;

            var pedido = mapper.Map<Pedido>(creacionPedidoDTO);
            pedido.ClienteId = clienteGuid;
            pedido.Estado = estadoPedido;

            pedido.pedidoDetalles = creacionPedidoDTO.Platos.Select(platoPedido => new PedidoDetalle
            {
                PlatoId = platoPedido.PlatoId,
                Cantidad = platoPedido.Cantidad,
                precioUnitario = platosDisponibles[platoPedido.PlatoId] // Precio tomado de la tabla Plato
            }).ToList();

            pedido.Total = pedido.pedidoDetalles.Sum(d => d.precioUnitario * d.Cantidad);

            context.Add(pedido);
            await context.SaveChangesAsync();
            var pedidoDTO = mapper.Map<PedidoDTO>(pedido);

            pedidoDTO.pedidoDetalles = mapper.Map<List<PedidoDetalleDTO>>(pedido.pedidoDetalles);

            return CreatedAtRoute("ObtenerPedidoId", new { id = pedido.Id }, pedidoDTO);

        }

        

        //[HttpPut("{id:int}")]
        //public async Task<ActionResult> Put(int id,CreacionPedidoDTO creacionPedidoDTO)
        //{

        //    var clienteExiste = await context.Clientes
        //        .AnyAsync(x => x.Id == creacionPedidoDTO.ClienteId);

        //    if (!clienteExiste)
        //    {
        //        return NotFound("el cliente no existe");
        //    }

        //    var pedido = await context.Pedidos
        //        .FirstOrDefaultAsync(x => x.Id == id);
        //    if(pedido == null)
        //    {
        //        return NotFound($"el pedido con el {id} no se encuentra en el sistema");
        //    }


        //    var pedidoDTO = mapper.Map(creacionPedidoDTO, pedido);
        //    await context.SaveChangesAsync();
        //    return NoContent();

        //}
      
    }
}
