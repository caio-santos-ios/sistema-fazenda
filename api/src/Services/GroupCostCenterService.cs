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
    public class GroupCostCenterService(IGroupCostCenterRepository repository) : IGroupCostCenterService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request)
        {
            try
            {
                Pagination<GroupCostCenter> pagination = new(request.QueryParams);
                
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
                        {"value", new BsonDocument ("$toDouble", "$value")},
                        {"created_at", 1},
                        {"updated_at", 1}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);

                return new(list, 200, "Grupos de centro de custo listados com sucesso");
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
                Pagination<GroupCostCenter> pagination = new(request.QueryParams);
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
                        {"name", 1}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);
                return new(list, 200, "Grupos listados para seleção");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }

        public async Task<ResponseApi<GroupCostCenter?>> GetByIdAsync(string id)
        {
            try
            {
                GroupCostCenter? entity = await repository.GetByIdAsync(id);
                if (entity is null) return new(null, 404, "Grupo não encontrado");

                return new(entity, 200, "Grupo encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<GroupCostCenter?>> CreateAsync(CreateGroupCostCenterRequest request)
        {
            try
            {
                GroupCostCenter entity = ObjectMapper.Map<CreateGroupCostCenterRequest, GroupCostCenter>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                long count = await repository.GetCountDocumentsAsync();
                long seq = count + 1;
                string code = GenerateCode.GenerateNextCode("", seq);
                while (await repository.GetByCodeAsync(code) != null)
                {
                    seq++;
                    code = GenerateCode.GenerateNextCode("", seq);
                }
                entity.Code = code;

                GroupCostCenter? created = await repository.CreateAsync(entity);
                if (created is null) return new(null, 400, "Falha ao criar grupo de centro de custo");

                return new(created, 201, "Grupo de centro de custo criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<GroupCostCenter?>> UpdateAsync(UpdateGroupCostCenterRequest request)
        {
            try
            {
                GroupCostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "Grupo não encontrado");

                string codeToUse = request.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(codeToUse))
                {
                    if (!string.IsNullOrWhiteSpace(existing.Code))
                    {
                        codeToUse = existing.Code;
                    }
                    else
                    {
                        long count = await repository.GetCountDocumentsAsync();
                        long seq = count + 1;
                        string generatedCode = seq.ToString();
                        while (await repository.GetByCodeAsync(generatedCode) != null)
                        {
                            seq++;
                            generatedCode = seq.ToString();
                        }
                        codeToUse = generatedCode;
                    }
                }

                if (!string.Equals(existing.Code, codeToUse, StringComparison.OrdinalIgnoreCase))
                {
                    var codeConflict = await repository.GetByCodeAsync(codeToUse);
                    if (codeConflict != null && codeConflict.Id != existing.Id)
                    {
                        return new(null, 400, $"Já existe outro Grupo com o código '{codeToUse}'.");
                    }
                }

                existing.Code = codeToUse;
                existing.Name = request.Name;
                existing.Value = request.Value;
                existing.UpdatedBy = request.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;

                GroupCostCenter? updated = await repository.UpdateAsync(existing);
                if (updated is null) return new(null, 400, "Falha ao atualizar grupo");

                return new(updated, 200, "Grupo atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<GroupCostCenter?>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                GroupCostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "Grupo não encontrado");

                existing.Deleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = request.DeletedBy;

                GroupCostCenter deleted = await repository.DeleteAsync(existing);
                return new(deleted, 204, "Grupo excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion
    }
}
