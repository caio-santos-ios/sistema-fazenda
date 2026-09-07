using System.ComponentModel.DataAnnotations;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Requests
{
    public class CreateGroupCostCenterRequest : RequestBase
    {
        [Display(Order = 1)]
        public string? Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [Display(Order = 2)]
        public string Name { get; set; } = string.Empty;

        [Display(Order = 3)]
        public decimal Value { get; set; } = 0m;
    }
}
