using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;
using api_bora_trampar.src.Utils;
using MongoDB.Bson;

namespace api_bora_trampar.src.Services
{
    public class SubCostCenterService(
        ISubCostCenterRepository repository,
        IGroupCostCenterRepository groupRepository,
        IGroupSubCostCenterRepository subGroupRepository) : ISubCostCenterService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request)
        {
            try
            {
                Pagination<SubCostCenter> pagination = new(request.QueryParams);
                
                List<BsonDocument> pipeline =
                [
                    new("$match", pagination.PipelineFilter),
                    new("$sort", pagination.PipelineSort),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"code", 1},
                        {"name", 1},
                        {"groupCode", 1},
                        {"subGroupCode", 1},
                        {"value", new BsonDocument ("$toDouble", "$value")},
                        {"created_at", 1},
                        {"updated_at", 1}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);

                return new(list, 200, "SubCentros de custo listados com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }

        public async Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllRequest request)
        {
            try
            {
                Pagination<SubCostCenter> pagination = new(request.QueryParams);
                var matchFilter = new BsonDocument("deleted", false);
                foreach (var el in pagination.PipelineFilter.Elements)
                {
                    if (el.Name != "deleted")
                    {
                        matchFilter.Add(el);
                    }
                }

                List<BsonDocument> pipeline =
                [
                    new("$match", matchFilter),
                    new("$sort", new BsonDocument("code", 1)),
                    new("$project", new BsonDocument
                    {
                        {"_id", 0},
                        {"id", new BsonDocument("$toString", "$_id")},
                        {"code", 1},
                        {"name", 1},
                        {"groupCode", 1},
                        {"subGroupCode", 1}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);
                return new(list, 200, "SubCentros de custo listados para seleção");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }

        public async Task<ResponseApi<SubCostCenter?>> GetByIdAsync(string id)
        {
            try
            {
                SubCostCenter? entity = await repository.GetByIdAsync(id);
                if (entity is null) return new(null, 404, "SubCentro de custo não encontrado");

                return new(entity, 200, "SubCentro de custo encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<SubCostCenter?>> CreateAsync(CreateSubCostCenterRequest request)
        {
            try
            {
                SubCostCenter entity = ObjectMapper.Map<CreateSubCostCenterRequest, SubCostCenter>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                long count = await repository.GetCountDocumentsAsync(request.GroupCode, request.SubGroupCode);
                long seq = count + 1;
                string code = GenerateCode.GenerateNextCode(request.SubGroupCode, seq);
                while (await repository.GetByCodeAsync(code) != null)
                {
                    seq++;
                    code = GenerateCode.GenerateNextCode(request.SubGroupCode, seq);
                }
                entity.Code = code;

                SubCostCenter? created = await repository.CreateAsync(entity);
                if (created is null) return new(null, 400, "Falha ao criar subcentro de custo");

                return new(created, 201, "SubCentro de custo criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<SubCostCenter?>> UpdateAsync(UpdateSubCostCenterRequest request)
        {
            try
            {
                SubCostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "SubCentro de custo não encontrado");

                var parentGroup = await groupRepository.GetByCodeAsync(request.GroupCode);
                if (parentGroup == null)
                {
                    return new(null, 400, $"Grupo com código '{request.GroupCode}' não foi encontrado.");
                }

                var parentSubGroup = await subGroupRepository.GetByCodeAsync(request.SubGroupCode);
                if (parentSubGroup == null)
                {
                    return new(null, 400, $"SubGrupo com código '{request.SubGroupCode}' não foi encontrado.");
                }

                string codeToUse = request.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(codeToUse))
                {
                    if (!string.IsNullOrWhiteSpace(existing.Code))
                    {
                        codeToUse = existing.Code;
                    }
                    else
                    {
                        long count = await repository.GetCountDocumentsAsync(request.GroupCode, request.SubGroupCode);
                        long seq = count + 1;
                        string generatedCode = GenerateCode.GenerateNextCode(request.SubGroupCode, seq);
                        while (await repository.GetByCodeAsync(generatedCode) != null)
                        {
                            seq++;
                            generatedCode = GenerateCode.GenerateNextCode(request.SubGroupCode, seq);
                        }
                        codeToUse = generatedCode;
                    }
                }

                if (!string.Equals(existing.Code, codeToUse, StringComparison.OrdinalIgnoreCase))
                {
                    var codeConflict = await repository.GetByCodeAsync(codeToUse);
                    if (codeConflict != null && codeConflict.Id != existing.Id)
                    {
                        return new(null, 400, $"Já existe outro SubCentro de custo com o código '{codeToUse}'.");
                    }
                }

                existing.Code = codeToUse;
                existing.Name = request.Name;
                existing.GroupCode = request.GroupCode;
                existing.SubGroupCode = request.SubGroupCode;
                existing.Value = request.Value;
                existing.UpdatedBy = request.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;

                SubCostCenter? updated = await repository.UpdateAsync(existing);
                if (updated is null) return new(null, 400, "Falha ao atualizar subcentro de custo");

                return new(updated, 200, "SubCentro de custo atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<SubCostCenter?>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                SubCostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "SubCentro de custo não encontrado");

                existing.Deleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = request.DeletedBy;

                SubCostCenter deleted = await repository.DeleteAsync(existing);
                return new(deleted, 204, "SubCentro de custo excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion
    }
}
