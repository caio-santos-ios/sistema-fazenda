using System.ComponentModel.DataAnnotations;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Requests
{
    public class CreateGroupSubCostCenterRequest : RequestBase
    {
        [Required(ErrorMessage = "O Código do Grupo é obrigatório.")]
        [Display(Order = 1)]
        public string GroupCode { get; set; } = string.Empty;

        [Display(Order = 2)]
        public string? Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [Display(Order = 3)]
        public string Name { get; set; } = string.Empty;

        [Display(Order = 4)]
        public decimal Value { get; set; } = 0m;
    }
}
