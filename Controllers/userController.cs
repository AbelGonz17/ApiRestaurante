using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestauranteAPI.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class userController:ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;

        public userController(ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, IConfiguration configuration, SignInManager<IdentityUser> signInManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuario)
        {
            var user = new IdentityUser
            {
                UserName = credencialesUsuario.name,
                Email = credencialesUsuario.Email
            };

            var resultado = await userManager.CreateAsync(user, credencialesUsuario.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }

            var cliente = new Cliente
            {
                Id = new Guid(user.Id),
                Nombre = credencialesUsuario.name,
                Email = credencialesUsuario.Email,
                FechaRegistro = DateTime.UtcNow
            };

            context.Add(cliente);
            await context.SaveChangesAsync();

           var respuestaAutenticacion = await ConstruirToken(credencialesUsuario);
            return respuestaAutenticacion;
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login (CrendecialesLoginDTO crendecialesLoginDTO)
        {
            var cliente = await userManager.FindByEmailAsync(crendecialesLoginDTO.Email);

            if(cliente == null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(cliente, crendecialesLoginDTO.Password, false);

            if(!resultado.Succeeded)
            {
                return RetornarLoginIncorrecto();
            }

            var credencialesUsuario = new CredencialesUsuarioDTO
            {
                Email = cliente.Email,
                Password = crendecialesLoginDTO.Password,
                name = cliente.UserName
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await context.Clientes
                .FirstOrDefaultAsync(x => x.Email == credencialesUsuarioDTO.Email);

            if (usuario == null)
            {
                throw new Exception("usuario no existe");
            }

            var claims = new List<Claim>
            {
                new Claim("Email",credencialesUsuarioDTO.Email),
                new Claim("UsuarioId", usuario.Id.ToString()),
            };

            var user = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsDB);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };

        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrecta");
            return ValidationProblem();
        }
    }
}
