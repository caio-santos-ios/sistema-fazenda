using System.ComponentModel.DataAnnotations;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Requests
{
    public class UpdateGroupSubCostCenterRequest : RequestBase
    {
        [Required(ErrorMessage = "O Id é obrigatório.")]
        [Display(Order = 1)]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Código do Grupo é obrigatório.")]
        [Display(Order = 2)]
        public string GroupCode { get; set; } = string.Empty;

        [Display(Order = 3)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [Display(Order = 4)]
        public string Name { get; set; } = string.Empty;

        [Display(Order = 5)]
        public decimal Value { get; set; } = 0m;
    }
}
