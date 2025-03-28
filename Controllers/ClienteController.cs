using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;

namespace RestauranteAPI.Controllers
{
    [ApiController]
    [Route("api/Cliente")]
    public class ClienteController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;

        public ClienteController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        public async Task<ActionResult<List<clienteDTO>>> Get()
        {
            var clientes = await context.Clientes
           .OrderBy(u => u.Nombre)
           .ProjectTo<clienteDTO>(mapper.ConfigurationProvider)
           .ToListAsync();

            return clientes;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<clienteDTO>> GetAction(Guid id)
        {
            var cliente = await context.Clientes
                .ProjectTo<clienteDTO>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }
            return cliente;
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<clientePatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();

            }

            var cliente = await context.Clientes.FirstOrDefaultAsync(x => x.Id == id);

            if (cliente == null)
            {
                return NotFound($"el Cliente con el Id {id} no se encuntra en el sistema");
            }

            var clienteDTO = mapper.Map<clientePatchDTO>(cliente);

            patchDocument.ApplyTo(clienteDTO, ModelState);

            var isValid = TryValidateModel(clienteDTO);

            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(clienteDTO, cliente);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existe = await context.Clientes.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Remove(new Cliente() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }


    }
}