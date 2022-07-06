using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filtros
{
    public class MiFiltroDeAccion : IActionFilter
    {
        public readonly ILogger<MiFiltroDeAccion> logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            this.logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context) //Se ejecuta antes de la acción
        {
            logger.LogInformation("Antes de ejecutar la acción");
        }
        public void OnActionExecuted(ActionExecutedContext context)//Se ejecuta después de que la acción se haya ejecutado
        {
            logger.LogInformation("Después de ejecutar la acción");
        }

        
    }
}
