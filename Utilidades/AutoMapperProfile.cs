using AutoMapper;
using Azure.Core;
using RestauranteAPI.DTOs;
using RestauranteAPI.Entidades;

namespace RestauranteAPI.Utilidades
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Cliente, clienteDTO>().ReverseMap();
            CreateMap<clientePatchDTO, Cliente>().ReverseMap();

            CreateMap<Plato, PlatoDTO>().ReverseMap();
            CreateMap<CreacionPlatoDTO, Plato>().ReverseMap();

            CreateMap<Pedido, PedidoDTO>()
                .ForMember(dest => dest.pedidoDetalles, opt => opt.MapFrom(src => src.pedidoDetalles)) // Mapeo de la lista
                .ReverseMap(); // Solo si lo necesitas

            // Mapeo entre PedidoDetalle y PedidoDetalleDTO
            CreateMap<PedidoDetalle, PedidoDetalleDTO>()
                .ForMember(dest => dest.PlatoNombre, opt => opt.MapFrom(src => src.Plato.Nombre)) // Mapeo de propiedades anidadas
                .ForMember(dest => dest.PlatoPrecio, opt => opt.MapFrom(src => src.Plato.precio))
                .ReverseMap();

            CreateMap<CreacionPedidoDTO, Pedido>()
                .ForMember(x => x.pedidoDetalles, opciones => opciones.MapFrom<PedidoDetalleResolver>());

            CreateMap<Pago, PagoDTO>().ReverseMap();
            CreateMap<CreacionPagoDTO, Pago>()
                    .AfterMap((src, dest, context) =>
                    {
                        // Recuperamos el objeto Pedido desde Items
                        var pedido = context.Items["Pedido"] as Pedido;

                        if (pedido != null)
                        {
                            dest.PedidoId = pedido.Id;
                            dest.Monto = pedido.Total;
                            dest.Fecha = DateTime.UtcNow;
                        }
                    });
        }

        //private List<PedidoDetalle> MapPedidoDetalle(CreacionPedidoDTO pedidoDTO, Pedido pedido)
        //{
        //    var resultado = new List<PedidoDetalle>();

        //    if(pedidoDTO.PlatosIds == null) { return resultado;  }

        //    foreach(var platoId in pedidoDTO.PlatosIds)
        //    {
        //        resultado.Add(new PedidoDetalle()
        //        {
        //            PedidoId = pedido.Id,
        //            PlatoId = platoId,
        //        });
        //    }

        //    return resultado;

        //}
        

    }
}
