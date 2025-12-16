using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class DatosCalculo
    {
        public double NumeroA { get; set; }
        public double NumeroB { get; set; }
        public string Operacion { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Api funcionando";
        }
        //  Agregamos el atributo HttpPost
        [HttpPost]
        public IActionResult RealizarCalculo([FromBody] DatosCalculo datos)
        {
            double resultado = 0;

            
            switch (datos.Operacion.ToLower())
            {
                case "suma":
                    resultado = datos.NumeroA + datos.NumeroB;
                    break;
                case "resta":
                    resultado = datos.NumeroA - datos.NumeroB;
                    break;
                case "multiplicar":
                    resultado = datos.NumeroA * datos.NumeroB;
                    break;
                case "dividir":
                    if (datos.NumeroB == 0) return BadRequest("No se puede dividir por cero.");
                    resultado = datos.NumeroA / datos.NumeroB;
                    break;
                default:
                    return BadRequest("Operación no válida. Intenta 'suma', 'resta', etc.");
            }
            return Ok(new
            {
                Mensaje = "Cálculo exitoso",
                Operacion = datos.Operacion,
                Total = resultado
            });
        }
    }
}
