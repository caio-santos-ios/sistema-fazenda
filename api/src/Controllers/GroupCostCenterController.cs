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
    [Route("api/group-cost-centers")]
    public class GroupCostCenterController(IGroupCostCenterService service) : ControllerBase
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
            ResponseApi<GroupCostCenter?> response = await service.GetByIdAsync(id);
            return StatusCode(response.StatusCode, new { response.Result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupCostCenterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            request.CreatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            request.UpdatedBy = request.CreatedBy;

            ResponseApi<GroupCostCenter?> response = await service.CreateAsync(request);
            return StatusCode(response.StatusCode, response.StatusCode == 201 ? new { response.Result } : new { message = response.Message });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGroupCostCenterRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            request.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

            ResponseApi<GroupCostCenter?> response = await service.UpdateAsync(request);
            return StatusCode(response.StatusCode, response.StatusCode == 200 ? new { response.Result } : new { message = response.Message });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            ResponseApi<GroupCostCenter?> response = await service.DeleteAsync(new() { Id = id, DeletedBy = userId });
            return StatusCode(response.StatusCode, new { response.Message });
        }
    }
}
