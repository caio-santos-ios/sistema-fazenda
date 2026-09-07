using System.Globalization;
using System.Text.RegularExpressions;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;
using api_bora_trampar.src.Utils;
using ClosedXML.Excel;
using MongoDB.Bson;

namespace api_bora_trampar.src.Services
{
    public class CostCenterService(
        ICostCenterRepository repository,
        IGroupCostCenterRepository groupRepository,
        IGroupSubCostCenterRepository subGroupRepository,
        ISubCostCenterRepository subCostCenterRepository,
        IImportHistoryRepository importHistoryRepository,
        IUserRepository userRepository) : ICostCenterService
    {
        #region READ
        public async Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request)
        {
            try
            {
                Pagination<CostCenter> pagination = new(request.QueryParams);
                
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
                        {"subCostCenter", new BsonDocument("$ifNull", new BsonArray { "$subCostCenter", "" })},
                        {"value", new BsonDocument ("$toDouble", "$value")},
                        {"created_at", 1},
                        {"updated_at", 1}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);

                return new(list, 200, "Centros de custo listados com sucesso");
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
                Pagination<CostCenter> pagination = new(request.QueryParams);
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
                        {"subGroupCode", 1},
                        {"subCostCenter", new BsonDocument("$ifNull", new BsonArray { "$subCostCenter", "" })}
                    })
                ];

                List<dynamic> list = await repository.GetAllAsync(pipeline);
                return new(list, 200, "Centros de custo listados para seleção");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }

        public async Task<ResponseApi<CostCenter?>> GetByIdAsync(string id)
        {
            try
            {
                CostCenter? entity = await repository.GetByIdAsync(id);
                if (entity is null) return new(null, 404, "Centro de custo não encontrado");

                return new(entity, 200, "Centro de custo encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region CREATE
        public async Task<ResponseApi<CostCenter?>> CreateAsync(CreateCostCenterRequest request)
        {
            try
            {
                string parentCode = !string.IsNullOrWhiteSpace(request.SubCostCenter)
                    ? request.SubCostCenter.Trim()
                    : request.SubGroupCode.Trim();

                CostCenter entity = ObjectMapper.Map<CreateCostCenterRequest, CostCenter>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                long count = await repository.GetCountDocumentsAsync(request.GroupCode, request.SubGroupCode, request.SubCostCenter ?? "");
                long seq = count + 1;
                string code = GenerateCode.GenerateNextCode(parentCode, seq);
                while (await repository.GetByCodeAsync(code) != null)
                {
                    seq++;
                    code = GenerateCode.GenerateNextCode(parentCode, seq);
                }
                entity.Code = code;

                CostCenter? created = await repository.CreateAsync(entity);
                if (created is null) return new(null, 400, "Falha ao criar centro de custo");

                return new(created, 201, "Centro de custo criado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region UPDATE
        public async Task<ResponseApi<CostCenter?>> UpdateAsync(UpdateCostCenterRequest request)
        {
            try
            {
                CostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "Centro de custo não encontrado");

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

                if (!string.IsNullOrWhiteSpace(request.SubCostCenter))
                {
                    var parentSubCostCenter = await subCostCenterRepository.GetByCodeAsync(request.SubCostCenter);
                    if (parentSubCostCenter == null)
                    {
                        return new(null, 400, $"SubCentro de custo com código '{request.SubCostCenter}' não foi encontrado.");
                    }
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
                        string parentCode = !string.IsNullOrWhiteSpace(request.SubCostCenter)
                            ? request.SubCostCenter.Trim()
                            : request.SubGroupCode.Trim();
                        long count = await repository.GetCountDocumentsAsync(request.GroupCode, request.SubGroupCode, request.SubCostCenter ?? "");
                        long seq = count + 1;
                        string generatedCode = GenerateCode.GenerateNextCode(parentCode, seq);
                        while (await repository.GetByCodeAsync(generatedCode) != null)
                        {
                            seq++;
                            generatedCode = GenerateCode.GenerateNextCode(parentCode, seq);
                        }
                        codeToUse = generatedCode;
                    }
                }

                if (!string.Equals(existing.Code, codeToUse, StringComparison.OrdinalIgnoreCase))
                {
                    var codeConflict = await repository.GetByCodeAsync(codeToUse);
                    if (codeConflict != null && codeConflict.Id != existing.Id)
                    {
                        return new(null, 400, $"Já existe outro Centro de custo com o código '{codeToUse}'.");
                    }
                }

                existing.Code = codeToUse;
                existing.Name = request.Name;
                existing.GroupCode = request.GroupCode;
                existing.SubGroupCode = request.SubGroupCode;
                existing.SubCostCenter = request.SubCostCenter;
                existing.Value = request.Value;
                existing.UpdatedBy = request.UpdatedBy;
                existing.UpdatedAt = DateTime.UtcNow;

                CostCenter? updated = await repository.UpdateAsync(existing);
                if (updated is null) return new(null, 400, "Falha ao atualizar centro de custo");

                return new(updated, 200, "Centro de custo atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region DELETE
        public async Task<ResponseApi<CostCenter?>> DeleteAsync(DeleteRequest request)
        {
            try
            {
                CostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "Centro de custo não encontrado");

                existing.Deleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                existing.DeletedBy = request.DeletedBy;

                CostCenter deleted = await repository.DeleteAsync(existing);
                return new(deleted, 204, "Centro de custo excluído com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Ocorreu um erro inesperado: {ex.Message}");
            }
        }
        #endregion

        #region IMPORT
        public async Task<ResponseApi<object?>> ImportSpreadsheetAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new(null, 400, "Nenhum arquivo de planilha foi enviado.");
                }

                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                List<Dictionary<string, string>> rowsList = [];
                List<string> columnHeaders = [];

                if (extension == ".xlsx" || extension == ".xls")
                {
                    using var stream = new MemoryStream();
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return new(null, 400, "A planilha não contém nenhuma aba.");
                    }

                    var range = worksheet.RangeUsed();
                    if (range == null)
                    {
                        return new(null, 400, "A planilha está vazia.");
                    }

                    int firstRow = range.FirstRow().RowNumber();
                    int lastRow = range.LastRow().RowNumber();
                    int firstColumn = range.FirstColumn().ColumnNumber();
                    int lastColumn = range.LastColumn().ColumnNumber();

                    var headerTerms = new[] {
                        "descri", "hist", "nome", "item", "centro", "cc", "conta", "servi", "produto",
                        "valor", "val", "vl", "vlr", "total", "saldo", "preço", "preco", "quantia",
                        "cod", "codigo", "código", "classificacao"
                    };

                    int headerRow = -1;
                    int dataStartRow = firstRow;

                    for (int r = firstRow; r <= Math.Min(firstRow + 15, lastRow); r++)
                    {
                        int termMatches = 0;
                        int filledCells = 0;
                        for (int c = firstColumn; c <= lastColumn; c++)
                        {
                            string cellVal = GetCellValueAsString(worksheet.Cell(r, c));
                            if (!string.IsNullOrWhiteSpace(cellVal))
                            {
                                filledCells++;
                                string norm = StringSimilarity.NormalizeText(cellVal);
                                if (headerTerms.Any(t => norm.Contains(t)))
                                {
                                    termMatches++;
                                }
                            }
                        }

                        if (termMatches >= 1 && filledCells >= 2)
                        {
                            headerRow = r;
                            dataStartRow = r + 1;
                            break;
                        }
                    }

                    if (headerRow != -1)
                    {
                        for (int col = firstColumn; col <= lastColumn; col++)
                        {
                            string header = GetCellValueAsString(worksheet.Cell(headerRow, col));
                            if (string.IsNullOrWhiteSpace(header))
                                header = $"Coluna_{col - firstColumn + 1}";
                            columnHeaders.Add(header);
                        }
                    }
                    else
                    {

                        for (int col = firstColumn; col <= lastColumn; col++)
                        {
                            columnHeaders.Add($"Coluna_{col - firstColumn + 1}");
                        }
                    }

                    for (int row = dataStartRow; row <= lastRow; row++)
                    {
                        var rowDict = new Dictionary<string, string>();
                        bool hasData = false;

                        for (int col = firstColumn; col <= lastColumn; col++)
                        {
                            int colIndex = col - firstColumn;
                            string colName = columnHeaders[colIndex];
                            string cellValue = GetCellValueAsString(worksheet.Cell(row, col));
                            rowDict[colName] = cellValue;
                            if (!string.IsNullOrEmpty(cellValue))
                                hasData = true;
                        }

                        if (hasData)
                        {
                            rowsList.Add(rowDict);
                        }
                    }
                }
                else if (extension == ".csv")
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    string? firstLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(firstLine))
                    {
                        return new(null, 400, "Arquivo CSV vazio.");
                    }

                    char delimiter = firstLine.Contains(';') ? ';' : ',';
                    var headerTerms = new[] { "descri", "hist", "nome", "item", "centro", "cc", "conta", "valor", "val", "saldo", "total", "cod" };
                    bool isFirstLineHeader = headerTerms.Any(t => StringSimilarity.NormalizeText(firstLine).Contains(t));

                    if (isFirstLineHeader)
                    {
                        columnHeaders = firstLine.Split(delimiter).Select((h, i) => string.IsNullOrWhiteSpace(h) ? $"Coluna_{i + 1}" : h.Trim()).ToList();
                    }
                    else
                    {
                        var firstParts = firstLine.Split(delimiter);
                        for (int i = 0; i < firstParts.Length; i++) columnHeaders.Add($"Coluna_{i + 1}");
                        var rowDict = new Dictionary<string, string>();
                        for (int i = 0; i < columnHeaders.Count; i++) rowDict[columnHeaders[i]] = i < firstParts.Length ? firstParts[i].Trim() : "";
                        rowsList.Add(rowDict);
                    }

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(delimiter);
                        var rowDict = new Dictionary<string, string>();
                        for (int i = 0; i < columnHeaders.Count; i++)
                        {
                            rowDict[columnHeaders[i]] = i < parts.Length ? parts[i].Trim() : "";
                        }
                        rowsList.Add(rowDict);
                    }
                }
                else
                {
                    return new(null, 400, "Formato não suportado. Envie um arquivo .xlsx ou .csv.");
                }

                string? descColumn = null;
                string? valColumn = null;

                var descKeywords = new[] { "descri", "hist", "nome", "item", "centro", "cc", "detalhe", "conta", "servi", "produto", "especificacao" };
                var valKeywords = new[] { "valor", "val", "vl", "vlr", "total", "saldo", "preço", "preco", "quantia", "debito", "credito" };

                var descScores = new Dictionary<string, int>();
                var valScores = new Dictionary<string, int>();

                foreach (var col in columnHeaders)
                {
                    descScores[col] = 0;
                    valScores[col] = 0;
                    string normHeader = StringSimilarity.NormalizeText(col);

                    if (descKeywords.Any(k => normHeader.Contains(k))) descScores[col] += 25;
                    if (valKeywords.Any(k => normHeader.Contains(k))) valScores[col] += 25;
                }

                int sampleCount = Math.Min(rowsList.Count, 50);
                for (int i = 0; i < sampleCount; i++)
                {
                    var row = rowsList[i];
                    foreach (var col in columnHeaders)
                    {
                        if (row.TryGetValue(col, out var text) && !string.IsNullOrWhiteSpace(text))
                        {
                            if (TryParseDecimal(text, out _))
                            {
                                valScores[col] += 3;
                            }
                            else if (text.Trim().Length >= 3 && text.Any(char.IsLetter))
                            {
                                descScores[col] += 3;
                            }
                        }
                    }
                }

                var sortedDesc = descScores.OrderByDescending(x => x.Value).ToList();
                var sortedVal = valScores.OrderByDescending(x => x.Value).ToList();

                descColumn = sortedDesc.FirstOrDefault().Key;
                valColumn = sortedVal.FirstOrDefault(x => x.Key != descColumn).Key ?? sortedVal.FirstOrDefault().Key;

                if (string.IsNullOrEmpty(descColumn) && columnHeaders.Count > 0) descColumn = columnHeaders[0];
                if (string.IsNullOrEmpty(valColumn) && columnHeaders.Count > 1) valColumn = columnHeaders[1];
                else if (string.IsNullOrEmpty(valColumn) && columnHeaders.Count == 1) valColumn = columnHeaders[0];

                var allCostCenters = await repository.GetActiveCostCentersAsync();
                var allSubCostCenters = await subCostCenterRepository.GetActiveSubCostCentersAsync();
                var allSubGroups = await subGroupRepository.GetActiveSubGroupsAsync();
                var allGroups = await groupRepository.GetActiveGroupsAsync();

                var subCostCenterByCode = allSubCostCenters.ToDictionary(x => x.Code, x => x);
                var subGroupByCode = allSubGroups.ToDictionary(x => x.Code, x => x);
                var groupByCode = allGroups.ToDictionary(x => x.Code, x => x);

                var detailedLines = new List<object>();

                var accumulatedCcMap = new Dictionary<string, (CostCenter cc, decimal accumulatedValue, List<string> sourceDescriptions, bool hasAmbiguity, List<object> alternativeCandidates)>();
                var unmatchedRows = new List<object>();

                foreach (var row in rowsList)
                {
                    string rawDesc = descColumn != null && row.TryGetValue(descColumn, out var d) ? d.Trim() : "";
                    string rawVal = valColumn != null && row.TryGetValue(valColumn, out var v) ? v.Trim() : "";

                    if (string.IsNullOrWhiteSpace(rawDesc))
                    {
                        foreach (var kvp in row)
                        {
                            if (kvp.Key != valColumn && !string.IsNullOrWhiteSpace(kvp.Value) && !TryParseDecimal(kvp.Value, out _) && kvp.Value.Any(char.IsLetter))
                            {
                                rawDesc = kvp.Value.Trim();
                                break;
                            }
                        }
                    }

                    if (!TryParseDecimal(rawVal, out decimal val))
                    {
                        val = 0m;
                        foreach (var kvp in row)
                        {
                            if (kvp.Key != descColumn && TryParseDecimal(kvp.Value, out decimal parsedVal))
                            {
                                val = parsedVal;
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(rawDesc) && val == 0m) continue;

                    var candidateMatches = new List<(CostCenter cc, double sim)>();

                    foreach (var cc in allCostCenters)
                    {
                        if (string.IsNullOrWhiteSpace(cc.Name)) continue;

                        double sim = StringSimilarity.CalculateSimilarity(rawDesc, cc.Name);
                        if (sim >= 0.45)
                        {
                            candidateMatches.Add((cc, sim));
                        }
                    }

                    candidateMatches = candidateMatches.OrderByDescending(x => x.sim).ToList();

                    CostCenter? bestMatch = candidateMatches.Count > 0 ? candidateMatches[0].cc : null;
                    double bestSimilarity = candidateMatches.Count > 0 ? candidateMatches[0].sim : 0.0;

                    bool hasAmbiguity = false;
                    var alternativeCandidates = new List<object>();

                    if (bestMatch != null && candidateMatches.Count > 1)
                    {

                        var closeCandidates = candidateMatches.Skip(1).Where(alt =>
                            alt.cc.Id != bestMatch.Id &&
                            (string.Equals(alt.cc.Name.Trim(), bestMatch.Name.Trim(), StringComparison.OrdinalIgnoreCase) ||
                             alt.sim >= (bestSimilarity - 0.08))
                        ).ToList();

                        if (closeCandidates.Count > 0)
                        {
                            hasAmbiguity = true;
                            alternativeCandidates = closeCandidates.Take(5).Select(alt =>
                            {
                                groupByCode.TryGetValue(alt.cc.GroupCode, out var g);
                                subGroupByCode.TryGetValue(alt.cc.SubGroupCode, out var sg);
                                subCostCenterByCode.TryGetValue(alt.cc.SubCostCenter ?? "", out var scc);

                                string path = $"{g?.Name ?? alt.cc.GroupCode} > {sg?.Name ?? alt.cc.SubGroupCode}";
                                if (scc != null) path += $" > {scc.Name}";

                                return new
                                {
                                    id = alt.cc.Id,
                                    code = alt.cc.Code,
                                    name = alt.cc.Name,
                                    groupCode = alt.cc.GroupCode,
                                    subGroupCode = alt.cc.SubGroupCode,
                                    subCostCenter = alt.cc.SubCostCenter ?? "",
                                    similarity = Math.Round(alt.sim, 2),
                                    path = path
                                };
                            }).Cast<object>().ToList();
                        }
                    }

                    if (bestMatch != null && bestSimilarity >= 0.45)
                    {
                        string srcDesc = $"{rawDesc} (R$ {val:N2})";

                        groupByCode.TryGetValue(bestMatch.GroupCode, out var g);
                        subGroupByCode.TryGetValue(bestMatch.SubGroupCode, out var sg);
                        subCostCenterByCode.TryGetValue(bestMatch.SubCostCenter ?? "", out var scc);
                        string hierarchyPath = $"{g?.Name ?? bestMatch.GroupCode} > {sg?.Name ?? bestMatch.SubGroupCode}";
                        if (scc != null) hierarchyPath += $" > {scc.Name}";

                        detailedLines.Add(new
                        {
                            costCenterId = bestMatch.Id,
                            costCenterCode = bestMatch.Code,
                            costCenterName = bestMatch.Name,
                            hierarchyPath = hierarchyPath,
                            sourceDescription = rawDesc,
                            value = val
                        });

                        if (!accumulatedCcMap.ContainsKey(bestMatch.Id))
                        {
                            accumulatedCcMap[bestMatch.Id] = (bestMatch, val, new List<string> { srcDesc }, hasAmbiguity, alternativeCandidates);
                        }
                        else
                        {
                            var existing = accumulatedCcMap[bestMatch.Id];
                            existing.sourceDescriptions.Add(srcDesc);
                            bool combinedAmbiguity = existing.hasAmbiguity || hasAmbiguity;
                            var combinedAlts = existing.alternativeCandidates.Count > 0 ? existing.alternativeCandidates : alternativeCandidates;
                            accumulatedCcMap[bestMatch.Id] = (bestMatch, existing.accumulatedValue + val, existing.sourceDescriptions, combinedAmbiguity, combinedAlts);
                        }
                    }
                    else
                    {
                        unmatchedRows.Add(new
                        {
                            description = rawDesc,
                            value = val,
                            bestMatchName = bestMatch?.Name,
                            similarity = Math.Round(bestSimilarity, 2)
                        });
                    }
                }

                var matchedCcList = accumulatedCcMap.Values.ToList();
                decimal totalMatchedValue = matchedCcList.Sum(x => x.accumulatedValue);

                var groupsInvolved = matchedCcList
                    .GroupBy(x => x.cc.GroupCode)
                    .Select(gGroup =>
                    {
                        string groupCode = gGroup.Key;
                        groupByCode.TryGetValue(groupCode, out var grp);
                        string groupName = grp?.Name ?? $"Grupo {groupCode}";
                        string groupId = grp?.Id ?? $"grp_{groupCode}";

                        var subGroupsInvolved = gGroup
                            .GroupBy(x => x.cc.SubGroupCode)
                            .Select(sgGroup =>
                            {
                                string subGroupCode = sgGroup.Key;
                                subGroupByCode.TryGetValue(subGroupCode, out var sg);
                                string subGroupName = sg?.Name ?? $"SubGrupo {subGroupCode}";
                                string subGroupId = sg?.Id ?? $"sg_{subGroupCode}";

                                var ccsWithSubCostCenter = sgGroup.Where(x => !string.IsNullOrWhiteSpace(x.cc.SubCostCenter)).ToList();
                                var directCcs = sgGroup.Where(x => string.IsNullOrWhiteSpace(x.cc.SubCostCenter)).ToList();

                                var subCostCentersInvolved = ccsWithSubCostCenter
                                    .GroupBy(x => x.cc.SubCostCenter!)
                                    .Select(sccGroup =>
                                    {
                                        string subCcCode = sccGroup.Key;
                                        subCostCenterByCode.TryGetValue(subCcCode, out var scc);
                                        string subCcName = scc?.Name ?? $"SubCentro {subCcCode}";
                                        string subCcId = scc?.Id ?? $"scc_{subCcCode}";

                                        var ccChildren = sccGroup.Select(item => new
                                        {
                                            id = item.cc.Id,
                                            costCenterId = item.cc.Id,
                                            code = item.cc.Code,
                                            name = item.cc.Name,
                                            type = "costcenter",
                                            level = 3,
                                            value = item.accumulatedValue,
                                            groupCode = item.cc.GroupCode,
                                            subGroupCode = item.cc.SubGroupCode,
                                            subCostCenter = item.cc.SubCostCenter,
                                            sourceDescriptions = item.sourceDescriptions,
                                            hasAmbiguity = item.hasAmbiguity,
                                            alternativeCandidates = item.alternativeCandidates,
                                            hasChildren = false,
                                            children = new List<object>()
                                        }).OrderBy(c => c.code).ToList();

                                        decimal sccTotal = ccChildren.Sum(c => c.value);

                                        return new
                                        {
                                            id = subCcId,
                                            code = subCcCode,
                                            name = subCcName,
                                            type = "subcostcenter",
                                            level = 2,
                                            value = sccTotal,
                                            groupCode = groupCode,
                                            subGroupCode = subGroupCode,
                                            hasChildren = ccChildren.Count > 0,
                                            children = ccChildren
                                        };
                                    }).ToList();

                                var directCcChildren = directCcs.Select(item => new
                                {
                                    id = item.cc.Id,
                                    costCenterId = item.cc.Id,
                                    code = item.cc.Code,
                                    name = item.cc.Name,
                                    type = "costcenter",
                                    level = 2,
                                    value = item.accumulatedValue,
                                    groupCode = item.cc.GroupCode,
                                    subGroupCode = item.cc.SubGroupCode,
                                    subCostCenter = "",
                                    sourceDescriptions = item.sourceDescriptions,
                                    hasAmbiguity = item.hasAmbiguity,
                                    alternativeCandidates = item.alternativeCandidates,
                                    hasChildren = false,
                                    children = new List<object>()
                                }).ToList();

                                var allSgChildren = new List<object>();
                                allSgChildren.AddRange(subCostCentersInvolved);
                                allSgChildren.AddRange(directCcChildren);

                                decimal sgTotal = allSgChildren.Sum(c => (decimal)((dynamic)c).value);

                                return new
                                {
                                    id = subGroupId,
                                    code = subGroupCode,
                                    name = subGroupName,
                                    type = "subgroup",
                                    level = 1,
                                    value = sgTotal,
                                    groupCode = groupCode,
                                    hasChildren = allSgChildren.Count > 0,
                                    children = allSgChildren
                                };
                            }).ToList();

                        decimal grpTotal = subGroupsInvolved.Sum(sg => (decimal)((dynamic)sg).value);

                        return new
                        {
                            id = groupId,
                            code = groupCode,
                            name = groupName,
                            type = "group",
                            level = 0,
                            value = grpTotal,
                            hasChildren = subGroupsInvolved.Count > 0,
                            children = subGroupsInvolved
                        };
                    }).OrderBy(g => g.code).ToList();

                var previewResult = new
                {
                    fileName = file.FileName,
                    totalRows = rowsList.Count,
                    totalColumns = columnHeaders.Count,
                    detectedDescriptionColumn = descColumn,
                    detectedValueColumn = valColumn,
                    matchedCount = matchedCcList.Count,
                    totalMatchedLines = detailedLines.Count,
                    ambiguousCount = matchedCcList.Count(m => m.hasAmbiguity),
                    unmatchedCount = unmatchedRows.Count,
                    totalMatchedValue = totalMatchedValue,
                    tree = groupsInvolved,
                    matchedItems = matchedCcList.Select(m => new {
                        id = m.cc.Id,
                        costCenterId = m.cc.Id,
                        code = m.cc.Code,
                        name = m.cc.Name,
                        value = m.accumulatedValue,
                        sourceDescriptions = m.sourceDescriptions,
                        hasAmbiguity = m.hasAmbiguity,
                        alternativeCandidates = m.alternativeCandidates
                    }),
                    detailedLines = detailedLines,
                    unmatchedRows = unmatchedRows
                };

                return new(previewResult, 200, $"Planilha processada! {matchedCcList.Count} Centros de Custo identificados com sucesso ({detailedLines.Count} linhas apuradas).");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao processar planilha: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object?>> ConfirmImportAsync(ConfirmImportRequest request, string userId)
        {
            try
            {
                if (request?.Items == null || request.Items.Count == 0)
                {
                    return new(null, 400, "Nenhum item informado para confirmação.");
                }

                var itemsToUpdate = request.Items
                    .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id)
                    .Select(g => (g.Key, g.Sum(x => x.Value), userId))
                    .ToList();

                long modified = await repository.BatchUpdateValuesAsync(itemsToUpdate);

                string userName = "Usuário";
                try
                {
                    var user = await userRepository.GetByIdAsync(userId);
                    if (user != null && !string.IsNullOrWhiteSpace(user.Name))
                    {
                        userName = user.Name;
                    }
                    else if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                    {
                        userName = user.Email;
                    }
                }
                catch { }

                var historyItems = (request.Lines != null && request.Lines.Count > 0)
                    ? request.Lines.Select(l => new ImportHistoryItem
                    {
                        CostCenterId = l.CostCenterId,
                        CostCenterCode = l.CostCenterCode,
                        CostCenterName = l.CostCenterName,
                        HierarchyPath = l.HierarchyPath,
                        SourceDescription = l.SourceDescription,
                        Value = l.Value
                    }).ToList()
                    : request.Items.Select(it => new ImportHistoryItem
                    {
                        CostCenterId = it.Id,
                        CostCenterCode = it.Code,
                        CostCenterName = it.Code,
                        HierarchyPath = "",
                        SourceDescription = it.Code,
                        Value = it.Value
                    }).ToList();

                var history = new ImportHistory
                {
                    FileName = string.IsNullOrWhiteSpace(request.FileName) ? "Planilha Importada" : request.FileName,
                    ImportedAt = DateTime.UtcNow,
                    ImportedBy = userId,
                    ImportedByName = userName,
                    TotalRows = request.TotalRows > 0 ? request.TotalRows : historyItems.Count,
                    MatchedCount = request.MatchedCount > 0 ? request.MatchedCount : itemsToUpdate.Count,
                    UnmatchedCount = request.UnmatchedCount,
                    TotalValue = request.TotalValue > 0 ? request.TotalValue : itemsToUpdate.Sum(x => x.Item2),
                    Items = historyItems
                };

                await importHistoryRepository.CreateAsync(history);

                return new(new { modifiedCount = modified, historyId = history.Id }, 200, $"{modified} Centros de Custo atualizados e histórico registrado com sucesso!");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao confirmar importação: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object?>> GetImportHistoriesAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                var histories = await importHistoryRepository.GetAllAsync(page, pageSize);
                long total = await importHistoryRepository.GetCountAsync();

                var list = histories.Select(h => new
                {
                    id = h.Id,
                    fileName = h.FileName,
                    importedAt = h.ImportedAt,
                    importedBy = h.ImportedBy,
                    importedByName = h.ImportedByName,
                    totalRows = h.TotalRows,
                    matchedCount = h.MatchedCount,
                    unmatchedCount = h.UnmatchedCount,
                    totalValue = h.TotalValue,
                    itemsCount = h.Items.Count
                }).ToList();

                return new(new { list, total, page, pageSize }, 200, "Histórico de importações listado com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao buscar histórico: {ex.Message}");
            }
        }

        public async Task<ResponseApi<ImportHistory?>> GetImportHistoryByIdAsync(string id)
        {
            try
            {
                var history = await importHistoryRepository.GetByIdAsync(id);
                if (history == null) return new(null, 404, "Histórico de importação não encontrado.");
                return new(history, 200, "Histórico encontrado.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao buscar detalhes do histórico: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object?>> DeleteImportHistoryAsync(string id, string userId)
        {
            try
            {
                var history = await importHistoryRepository.GetByIdAsync(id);
                if (history == null) return new(null, 404, "Histórico de importação não encontrado.");

                if (history.Items != null && history.Items.Count > 0)
                {
                    var itemsToDeduct = history.Items
                        .Where(x => !string.IsNullOrWhiteSpace(x.CostCenterId))
                        .GroupBy(x => x.CostCenterId)
                        .Select(g => (CostCenterId: g.Key, DeductValue: g.Sum(x => x.Value)))
                        .ToList();

                    foreach (var item in itemsToDeduct)
                    {
                        var cc = await repository.GetByIdAsync(item.CostCenterId);
                        if (cc != null)
                        {
                            cc.Value = Math.Max(0, cc.Value - item.DeductValue);
                            cc.UpdatedAt = DateTime.UtcNow;
                            cc.UpdatedBy = userId;
                            await repository.UpdateAsync(cc);
                        }
                    }
                }

                await importHistoryRepository.DeleteAsync(id);
                return new(new { id }, 200, "Importação removida e valores estornados com sucesso.");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao remover importação: {ex.Message}");
            }
        }
        #endregion

        #region MOVE
        public async Task<ResponseApi<CostCenter?>> MoveCostCenterAsync(MoveCostCenterRequest request, string userId)
        {
            try
            {
                CostCenter? existing = await repository.GetByIdAsync(request.Id);
                if (existing is null) return new(null, 404, "Centro de custo não encontrado.");

                var parentGroup = await groupRepository.GetByCodeAsync(request.TargetGroupCode);
                if (parentGroup == null)
                    return new(null, 400, $"Grupo de destino '{request.TargetGroupCode}' não encontrado.");

                var parentSubGroup = await subGroupRepository.GetByCodeAsync(request.TargetSubGroupCode);
                if (parentSubGroup == null)
                    return new(null, 400, $"SubGrupo de destino '{request.TargetSubGroupCode}' não encontrado.");

                if (!string.IsNullOrWhiteSpace(request.TargetSubCostCenter))
                {
                    var parentSubCc = await subCostCenterRepository.GetByCodeAsync(request.TargetSubCostCenter);
                    if (parentSubCc == null)
                        return new(null, 400, $"SubCentro de destino '{request.TargetSubCostCenter}' não encontrado.");
                }

                string newCode = existing.Code;
                bool isSameParent = string.Equals(existing.GroupCode, request.TargetGroupCode, StringComparison.OrdinalIgnoreCase) &&
                                    string.Equals(existing.SubGroupCode, request.TargetSubGroupCode, StringComparison.OrdinalIgnoreCase) &&
                                    string.Equals(existing.SubCostCenter ?? "", request.TargetSubCostCenter ?? "", StringComparison.OrdinalIgnoreCase);

                if (!isSameParent)
                {
                    newCode = await GenerateAvailableCodeAsync(request.TargetGroupCode, request.TargetSubGroupCode, request.TargetSubCostCenter);
                }

                existing.GroupCode = request.TargetGroupCode;
                existing.SubGroupCode = request.TargetSubGroupCode;
                existing.SubCostCenter = string.IsNullOrWhiteSpace(request.TargetSubCostCenter) ? null : request.TargetSubCostCenter.Trim();
                existing.Code = newCode;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId;

                CostCenter? updated = await repository.UpdateAsync(existing);
                if (updated is null) return new(null, 400, "Falha ao mover centro de custo.");

                return new(updated, 200, $"Centro de custo movido com sucesso para o código {newCode}!");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao mover centro de custo: {ex.Message}");
            }
        }

        public async Task<ResponseApi<object?>> PreviewMoveCostCenterAsync(string id, string targetGroup, string targetSubGroup, string? targetSubCostCenter)
        {
            try
            {
                CostCenter? existing = await repository.GetByIdAsync(id);
                if (existing is null) return new(null, 404, "Centro de custo não encontrado.");

                var grp = await groupRepository.GetByCodeAsync(targetGroup);
                var subGrp = await subGroupRepository.GetByCodeAsync(targetSubGroup);
                var subCc = !string.IsNullOrWhiteSpace(targetSubCostCenter) ? await subCostCenterRepository.GetByCodeAsync(targetSubCostCenter) : null;

                string previewCode = await GenerateAvailableCodeAsync(targetGroup, targetSubGroup, targetSubCostCenter);

                var preview = new
                {
                    currentCode = existing.Code,
                    currentName = existing.Name,
                    newCode = previewCode,
                    targetGroupName = grp?.Name ?? targetGroup,
                    targetSubGroupName = subGrp?.Name ?? targetSubGroup,
                    targetSubCostCenterName = subCc?.Name ?? targetSubCostCenter
                };

                return new(preview, 200, "Prévia gerada com sucesso");
            }
            catch (Exception ex)
            {
                return new(null, 500, $"Erro ao gerar prévia: {ex.Message}");
            }
        }

        private async Task<string> GenerateAvailableCodeAsync(string groupCode, string subGroupCode, string? subCostCenter)
        {
            string parentCode = !string.IsNullOrWhiteSpace(subCostCenter)
                ? subCostCenter.Trim()
                : subGroupCode.Trim();

            long count = await repository.GetCountDocumentsAsync(groupCode, subGroupCode, subCostCenter ?? "");
            long seq = count + 1;
            string generatedCode = GenerateCode.GenerateNextCode(parentCode, seq);

            while (await repository.GetByCodeAsync(generatedCode) != null)
            {
                seq++;
                generatedCode = GenerateCode.GenerateNextCode(parentCode, seq);
            }

            return generatedCode;
        }
        #endregion

        #region HELPERS
        private static string GetCellValueAsString(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return string.Empty;
            try
            {
                string formatted = cell.GetFormattedString();
                if (!string.IsNullOrWhiteSpace(formatted)) return formatted.Trim();
            }
            catch { }

            try
            {
                if (cell.Value.IsNumber)
                {
                    return cell.Value.GetNumber().ToString(CultureInfo.InvariantCulture);
                }
                return cell.Value.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool TryParseDecimal(string? input, out decimal result)
        {
            result = 0m;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string clean = input.Trim()
                .Replace("R$", "", StringComparison.OrdinalIgnoreCase)
                .Replace("%", "")
                .Replace(" ", "")
                .Trim();

            bool isNegative = false;
            if (clean.StartsWith('(') && clean.EndsWith(')'))
            {
                isNegative = true;
                clean = clean.Substring(1, clean.Length - 2).Trim();
            }

            if (clean.Contains(',') && clean.Contains('.'))
            {
                int commaIdx = clean.LastIndexOf(',');
                int dotIdx = clean.LastIndexOf('.');
                if (commaIdx > dotIdx)
                {

                    clean = clean.Replace(".", "").Replace(',', '.');
                }
                else
                {

                    clean = clean.Replace(",", "");
                }
            }
            else if (clean.Contains(','))
            {

                clean = clean.Replace(',', '.');
            }

            bool parsed = decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
            if (parsed && isNegative)
            {
                result = -result;
            }
            return parsed;
        }
        #endregion
    }
}
