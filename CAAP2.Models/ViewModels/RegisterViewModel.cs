using System.ComponentModel.DataAnnotations;

namespace CAAP2.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        public string Email { get; set; } = null!;

        public bool IsPremium { get; set; }

        public bool IsEndUser { get; set; }
    }
}
