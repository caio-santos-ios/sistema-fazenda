using System.ComponentModel.DataAnnotations;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Requests
{
    public class UpdateGroupCostCenterRequest : RequestBase
    {
        [Required(ErrorMessage = "O Id é obrigatório.")]
        [Display(Order = 1)]
        public string Id { get; set; } = string.Empty;

        [Display(Order = 2)]
        public string? Code { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [Display(Order = 3)]
        public string Name { get; set; } = string.Empty;

        [Display(Order = 4)]
        public decimal Value { get; set; } = 0m;
    }
}
