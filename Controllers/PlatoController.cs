using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;

namespace RestauranteAPI.Controllers
{
    [ApiController]
    [Route("api/platos")]
    public class PlatoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;

        public PlatoController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlatoDTO>>> Get()
        {
            var plato = await context.Platos.
               OrderBy(x => x.Nombre)
               .ProjectTo<PlatoDTO>(mapper.ConfigurationProvider)
               .ToListAsync();

            return plato;
        }

        [HttpGet("{id:int}", Name = "ObtenerPlatoId")]
        public async Task<ActionResult<PlatoDTO>> GetId(int id)
        {
            var plato = await context.Platos.FirstOrDefaultAsync(x => x.Id == id);

            if (plato == null)
            {
                return NotFound("No existe el plato con el id ingresado");
            }

            return mapper.Map<PlatoDTO>(plato);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreacionPlatoDTO creacionPlatoDTO)
        {
            if (creacionPlatoDTO == null)
            {
                return BadRequest("Datos invalidos");
            }

            var existePlato = await context.Platos.AnyAsync(x => x.Nombre == creacionPlatoDTO.Nombre);

            if (existePlato)
            {
                return BadRequest("Ya existe un plato con ese nombre");
            }

            var newPlato = mapper.Map<Plato>(creacionPlatoDTO);
            context.Add(newPlato);
            await context.SaveChangesAsync();

            var platoDTO = mapper.Map<PlatoDTO>(newPlato);

            return CreatedAtRoute("ObtenerPlatoId", new { id = platoDTO.Id }, platoDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CreacionPlatoDTO creacionPlatoDTO)
        {
            var plato = await context.Platos.FirstOrDefaultAsync(x => x.Id == id);
            if (plato == null)
            {
                return NotFound("No existe el plato con el id ingresado");
            }

            plato = mapper.Map(creacionPlatoDTO, plato);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var plato = await context.Platos.FirstOrDefaultAsync(x => x.Id == id);
            if (plato == null)
            {
                return NotFound("No existe el plato con el id ingresado");
            }
            context.Remove(plato);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
