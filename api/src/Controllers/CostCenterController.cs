using System.Security.Claims;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models._Base;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api_bora_trampar.src.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cost-centers")]
    public class CostCenterController(ICostCenterService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            GetAllRequest request = new(Request.Query);
            ResponseApi<List<dynamic>> response = await service.GetAllAsync(request);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [HttpGet("select")]
        public async Task<IActionResult> GetSelect()
        {
            GetAllRequest request = new(Request.Query);
            ResponseApi<List<dynamic>> response = await service.GetSelectAsync(request);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ResponseApi<CostCenter?> response = await service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCostCenterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            request.CreatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            request.UpdatedBy = request.CreatedBy;

            ResponseApi<CostCenter?> response = await service.CreateAsync(request);
            return StatusCode(response.StatusCode, response.StatusCode == 201 ? new { response.Result } : new { message = response.Message });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCostCenterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            request.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            ResponseApi<CostCenter?> response = await service.UpdateAsync(request);
            return StatusCode(response.StatusCode, response.StatusCode == 200 ? new { response.Result } : new { message = response.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            ResponseApi<CostCenter?> response = await service.DeleteAsync(new() { Id = id, DeletedBy = userId });
            return StatusCode(response.StatusCode, new { response.Message });
        }

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Arquivo não enviado ou vazio." });

            ResponseApi<object?> response = await service.ImportSpreadsheetAsync(file);
            return StatusCode(response.StatusCode, new { message = response.Message, result = response.Data });
        }

        [HttpPost("import/confirm")]
        public async Task<IActionResult> ConfirmImport([FromBody] ConfirmImportRequest request)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest(new { message = "Nenhum item selecionado para importação." });

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            ResponseApi<object?> response = await service.ConfirmImportAsync(request, userId);
            return StatusCode(response.StatusCode, new { message = response.Message, result = response.Data });
        }

        [HttpPost("move")]
        public async Task<IActionResult> Move([FromBody] MoveCostCenterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            ResponseApi<CostCenter?> response = await service.MoveCostCenterAsync(request, userId);
            return StatusCode(response.StatusCode, response.StatusCode == 200 ? new { message = response.Message, result = response.Data } : new { message = response.Message });
        }

        [HttpGet("move/preview")]
        public async Task<IActionResult> PreviewMove([FromQuery] string id, [FromQuery] string targetGroup, [FromQuery] string targetSubGroup, [FromQuery] string? targetSubCostCenter = null)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(targetGroup) || string.IsNullOrWhiteSpace(targetSubGroup))
                return BadRequest("Parâmetros insuficientes para a prévia.");

            ResponseApi<object?> response = await service.PreviewMoveCostCenterAsync(id, targetGroup, targetSubGroup, targetSubCostCenter);
            return StatusCode(response.StatusCode, new { result = response.Data });
        }

        [HttpGet("import/history")]
        public async Task<IActionResult> GetImportHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var response = await service.GetImportHistoriesAsync(page, pageSize);
            return StatusCode(response.StatusCode, new { message = response.Message, result = response.Data });
        }

        [HttpGet("import/history/{id}")]
        public async Task<IActionResult> GetImportHistoryById(string id)
        {
            var response = await service.GetImportHistoryByIdAsync(id);
            return StatusCode(response.StatusCode, response.StatusCode == 200 ? new { message = response.Message, result = response.Data } : new { message = response.Message });
        }

        [HttpDelete("import/history/{id}")]
        public async Task<IActionResult> DeleteImportHistory(string id)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            var response = await service.DeleteImportHistoryAsync(id, userId);
            return StatusCode(response.StatusCode, new { message = response.Message, result = response.Data });
        }
    }
}
