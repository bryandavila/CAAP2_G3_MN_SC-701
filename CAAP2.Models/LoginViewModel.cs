using System.ComponentModel.DataAnnotations;

namespace CAAP2_G3_MN_SC_701.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        public string Email { get; set; } = string.Empty;
    }
}
