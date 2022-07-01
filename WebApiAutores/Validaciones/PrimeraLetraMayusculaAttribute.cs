using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) //si esta no han puesto nada o viene vacio no hay que validar nada
            {
                return ValidationResult.Success; 
            }

            var primeraLetra = value.ToString()[0].ToString(); //extraemos la primera letra del string que nos llega (value)

            if(primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe ser mayúscula");
            }

            return ValidationResult.Success;
        }
    }
}
