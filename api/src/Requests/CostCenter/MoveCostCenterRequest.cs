using System.ComponentModel.DataAnnotations;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Requests
{
    public class MoveCostCenterRequest : RequestBase
    {
        [Required(ErrorMessage = "O Id do Centro de Custo é obrigatório.")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Grupo de destino é obrigatório.")]
        public string TargetGroupCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "O SubGrupo de destino é obrigatório.")]
        public string TargetSubGroupCode { get; set; } = string.Empty;

        public string? TargetSubCostCenter { get; set; }
    }
}