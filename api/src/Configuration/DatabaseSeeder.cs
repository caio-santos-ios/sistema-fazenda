using api_bora_trampar.src.Models;
using MongoDB.Driver;

namespace api_bora_trampar.src.Configuration
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            var now = DateTime.UtcNow;

            var groups = new (string Code, string Name)[]
            {
                ("1", "Custos Operacionais Diretos (COE)"),
                ("2", "Custos Operacionais Indiretos (COT)"),
                ("3", "Investimentos (CAPEX)"),
                ("4", "Compromissos e Dívidas"),
                ("5", "Saídas Não Operacionais"),
                ("6", "Investimentos Não Operacionais")
            };

            foreach (var g in groups)
            {
                var filter = Builders<GroupCostCenter>.Filter.Eq(x => x.Code, g.Code);
                var update = Builders<GroupCostCenter>.Update
                    .Set(x => x.Name, g.Name)
                    .Set(x => x.Deleted, false)
                    .SetOnInsert(x => x.Value, 0m)
                    .SetOnInsert(x => x.CreatedAt, now)
                    .SetOnInsert(x => x.CreatedBy, "system_seed");

                await db.GroupCostCenters.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }

            var subGroups = new (string Code, string Name, string GroupCode)[]
            {
                ("1.1", "Agricultura", "1"),
                ("1.2", "Pecuária", "1"),
                ("2.1", "Administração e Gestão", "2"),
                ("2.2", "Salários e Encargos", "2"),
                ("2.3", "Frota, Máquinas e Combustíveis", "2"),
                ("2.4", "Manutenção e Infraestrutura", "2"),
                ("3.1", "Máquinas, Implementos e Frota", "3"),
                ("3.2", "Infraestrutura e Benfeitorias Gerais / Agricultura", "3"),
                ("3.3", "Investimentos Específicos em Pecuária", "3"),
                ("4.1", "Custos de Outras Safras (Passadas)", "4"),
                ("4.2", "Dívidas e Passivos Estruturais", "4"),
                ("5.1", "Financeiras e Patrimoniais", "5"),
                ("6.1", "Ativos Fora da Atividade Rural Direta", "6")
            };

            foreach (var sg in subGroups)
            {
                var filter = Builders<GroupSubCostCenter>.Filter.Eq(x => x.Code, sg.Code);
                var update = Builders<GroupSubCostCenter>.Update
                    .Set(x => x.Name, sg.Name)
                    .Set(x => x.GroupCode, sg.GroupCode)
                    .Set(x => x.Deleted, false)
                    .SetOnInsert(x => x.Value, 0m)
                    .SetOnInsert(x => x.CreatedAt, now)
                    .SetOnInsert(x => x.CreatedBy, "system_seed");

                await db.GroupSubCostCenters.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }

            var subCostCenters = new (string Code, string Name, string GroupCode, string SubGroupCode)[]
            {
                ("1.1.1", "Geral (Custos Agrícolas Compartilhados)", "1", "1.1"),
                ("1.1.2", "Cultura: Soja", "1", "1.1"),
                ("1.1.3", "Cultura: Milho", "1", "1.1"),
                ("1.1.4", "Cultura: Algodão", "1", "1.1"),
                ("1.1.5", "Cultura: Arroz", "1", "1.1"),
                ("1.1.6", "Cultura: Gergelim", "1", "1.1"),
                ("1.1.7", "Cultura: Feijão", "1", "1.1")
            };

            foreach (var scc in subCostCenters)
            {
                var filter = Builders<SubCostCenter>.Filter.Eq(x => x.Code, scc.Code);
                var update = Builders<SubCostCenter>.Update
                    .Set(x => x.Name, scc.Name)
                    .Set(x => x.GroupCode, scc.GroupCode)
                    .Set(x => x.SubGroupCode, scc.SubGroupCode)
                    .Set(x => x.Deleted, false)
                    .SetOnInsert(x => x.Value, 0m)
                    .SetOnInsert(x => x.CreatedAt, now)
                    .SetOnInsert(x => x.CreatedBy, "system_seed");

                await db.SubCostCenters.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }

            var costCenters = new (string Code, string Name, string GroupCode, string SubGroupCode, string? SubCostCenter)[]
            {
                ("1.1.1.1", "Análise laboratorial de solo, folhas e nematoides", "1", "1.1", "1.1.1"),
                ("1.1.1.2", "Assessoria e consultoria agronômica, técnica e comercialização de safras", "1", "1.1", "1.1.1"),
                ("1.1.1.3", "Armazenagem, transbordo, secagem e limpeza de grãos", "1", "1.1", "1.1.1"),
                ("1.1.1.4", "Fretes de transferência interna e logística de apoio agrícola", "1", "1.1", "1.1.1"),

                ("1.1.2.1", "Sementes e tratamento industrial de sementes (TSI)", "1", "1.1", "1.1.2"),
                ("1.1.2.2", "Corretivos, fertilizantes de base, cobertura e adubação foliar", "1", "1.1", "1.1.2"),
                ("1.1.2.3", "Defensivos agrícolas (fungicidas, herbicidas, inseticidas, adjuvantes e biológicos)", "1", "1.1", "1.1.2"),
                ("1.1.2.4", "Operação mecanizada própria e aviação agrícola contratada", "1", "1.1", "1.1.2"),
                ("1.1.2.5", "Serviços terceirizados de apoio e mão de obra temporária de safra", "1", "1.1", "1.1.2"),
                ("1.1.2.6", "Arrendamento de terras vinculadas à cultura", "1", "1.1", "1.1.2"),
                ("1.1.2.7", "Taxas, impostos e contribuições sobre a venda da produção", "1", "1.1", "1.1.2"),
                ("1.1.2.8", "Juros e encargos financeiros de custeio agrícola específico", "1", "1.1", "1.1.2"),

                ("1.1.3.1", "Sementes e tratamento industrial de sementes (TSI)", "1", "1.1", "1.1.3"),
                ("1.1.3.2", "Corretivos e fertilizantes (NPK e micronutrientes)", "1", "1.1", "1.1.3"),
                ("1.1.3.3", "Defensivos agrícolas (químicos e controle biológico)", "1", "1.1", "1.1.3"),
                ("1.1.3.4", "Operação mecanizada e colheita terceirizada", "1", "1.1", "1.1.3"),
                ("1.1.3.5", "Serviços terceirizados e diaristas de campo", "1", "1.1", "1.1.3"),
                ("1.1.3.6", "Arrendamento de terras", "1", "1.1", "1.1.3"),
                ("1.1.3.7", "Taxas e impostos sobre a venda", "1", "1.1", "1.1.3"),
                ("1.1.3.8", "Juros de custeio agrícola", "1", "1.1", "1.1.3"),

                ("1.1.4.1", "Sementes certificadas e biotecnologia", "1", "1.1", "1.1.4"),
                ("1.1.4.2", "Corretivos e fertilizantes de alta solubilidade", "1", "1.1", "1.1.4"),
                ("1.1.4.3", "Defensivos, maturadores, desfolhantes e reguladores de crescimento", "1", "1.1", "1.1.4"),
                ("1.1.4.4", "Operação mecanizada, colheita e enfardamento de pluma", "1", "1.1", "1.1.4"),
                ("1.1.4.5", "Serviços de descaroçamento, beneficiamento e classificação de fibra", "1", "1.1", "1.1.4"),
                ("1.1.4.6", "Arrendamento de áreas de produção", "1", "1.1", "1.1.4"),
                ("1.1.4.7", "Taxas e impostos incidentes sobre a comercialização", "1", "1.1", "1.1.4"),
                ("1.1.4.8", "Juros de custeio agrícola", "1", "1.1", "1.1.4"),

                ("1.1.5.1", "Sementes e tratamento", "1", "1.1", "1.1.5"),
                ("1.1.5.2", "Fertilizantes de plantio e adubação nitrogenada", "1", "1.1", "1.1.5"),
                ("1.1.5.3", "Defensivos agrícolas e herbicidas específicos", "1", "1.1", "1.1.5"),
                ("1.1.5.4", "Operação mecanizada, confecção de taipas e nivelamento", "1", "1.1", "1.1.5"),
                ("1.1.5.5", "Manejo de irrigação, energia de bombeamento hídrico e canais", "1", "1.1", "1.1.5"),
                ("1.1.5.6", "Arrendamento de áreas de várzea ou coxilha", "1", "1.1", "1.1.5"),
                ("1.1.5.7", "Taxas e impostos sobre a comercialização", "1", "1.1", "1.1.5"),
                ("1.1.5.8", "Juros de custeio agrícola", "1", "1.1", "1.1.5"),

                ("1.1.6.1", "Sementes e insumos biológicos de plantio", "1", "1.1", "1.1.6"),
                ("1.1.6.2", "Fertilizantes e adubação foliar complementar", "1", "1.1", "1.1.6"),
                ("1.1.6.3", "Defensivos e dessecação de uniformização pré-colheita", "1", "1.1", "1.1.6"),
                ("1.1.6.4", "Operação mecanizada de semeadura e colheita", "1", "1.1", "1.1.6"),
                ("1.1.6.5", "Serviços terceirizados e mão de obra de apoio", "1", "1.1", "1.1.6"),
                ("1.1.6.6", "Arrendamento proporcional da área plantada", "1", "1.1", "1.1.6"),
                ("1.1.6.7", "Taxas e impostos sobre a venda", "1", "1.1", "1.1.6"),
                ("1.1.6.8", "Juros de custeio agrícola", "1", "1.1", "1.1.6"),

                ("1.1.7.1", "Sementes comerciais e inoculantes bacterianos", "1", "1.1", "1.1.7"),
                ("1.1.7.2", "Fertilizantes de base e adubação de cobertura", "1", "1.1", "1.1.7"),
                ("1.1.7.3", "Defensivos, inseticidas e fungicidas de manejo intensivo", "1", "1.1", "1.1.7"),
                ("1.1.7.4", "Operação mecanizada e aplicação de irrigação", "1", "1.1", "1.1.7"),
                ("1.1.7.5", "Serviços de colheita, recolhimento e beneficiamento", "1", "1.1", "1.1.7"),
                ("1.1.7.6", "Arrendamento de terras", "1", "1.1", "1.1.7"),
                ("1.1.7.7", "Taxas e impostos sobre a venda", "1", "1.1", "1.1.7"),
                ("1.1.7.8", "Juros de custeio agrícola", "1", "1.1", "1.1.7"),

                ("1.2.1", "Nutrição animal (sal mineral, proteinados, rações balanceadas, silagem e concentrados)", "1", "1.2", null),
                ("1.2.2", "Sanidade e saúde animal (vacinas oficiais, vermífugos, antibióticos, carrapaticidas e antissépticos)", "1", "1.2", null),
                ("1.2.3", "Reprodução e melhoramento genético (sêmen, nitrogênio líquido, bainhas e protocolos de IATF)", "1", "1.2", null),
                ("1.2.4", "Despesas comerciais (comissões de leiloeiras, corretagem de compra/venda, emissão de GTA e exames)", "1", "1.2", null),
                ("1.2.5", "Identificação e rastreabilidade (brincos Sisbov, botons visuais e chips eletrônicos)", "1", "1.2", null),
                ("1.2.6", "Frete boiadeiro e logística de transferência entre fazendas ou pastos", "1", "1.2", null),
                ("1.2.7", "Operação mecanizada pecuária (trato no cocho, distribuição de insumos e roçagem de pastos)", "1", "1.2", null),
                ("1.2.8", "Arrendamento e aluguel de pastagens de terceiros", "1", "1.2", null),
                ("1.2.9", "Taxas e impostos sobre comercialização de gado (Funrural e tributos estaduais)", "1", "1.2", null),
                ("1.2.10", "Manutenção de pastagem (adubação periódica, combate a plantas invasoras e pragas)", "1", "1.2", null),
                ("1.2.11", "Tropa e animais de serviço (arreios, nutrição de equinos, ferrageamento e medicamentos veterinários)", "1", "1.2", null),
                ("1.2.12", "Descarte sanitário e destinação de resíduos (recolha de carcaças e incineração de frascos)", "1", "1.2", null),

                ("2.1.1", "Serviços profissionais terceirizados (honorários contábeis, assessoria jurídica, auditoria e certificações)", "2", "2.1", null),
                ("2.1.2", "Tecnologia, conectividade e sistemas (links Starlink, internet rural, ERPs agrícolas, licenças e telefonia)", "2", "2.1", null),
                ("2.1.3", "Despesas de escritório e predial administrativo (papelaria, materiais de limpeza e energia da sede)", "2", "2.1", null),
                ("2.1.4", "Tarifas bancárias, taxas de expediente, manutenção de contas e custódia financeira", "2", "2.1", null),
                ("2.1.5", "Viagens, deslocamentos executivos, hospedagem, alimentação e representação institucional", "2", "2.1", null),
                ("2.1.6", "Seguros patrimoniais e prediais das estruturas administrativas", "2", "2.1", null),

                ("2.2.1", "Folha de pagamento fixa do corpo operacional, lideranças de campo e encarregados", "2", "2.2", null),
                ("2.2.2", "Encargos sociais e trabalhistas obrigatórios (INSS, FGTS, guia GPS e contribuições sindicais)", "2", "2.2", null),
                ("2.2.3", "Benefícios corporativos (alimentação, rancho de apoio, alojamento e planos de saúde)", "2", "2.2", null),
                ("2.2.4", "Provisões legais e verbas rescisórias (férias com 1/3 constitucional, 13º salário e rescisões)", "2", "2.2", null),
                ("2.2.5", "Segurança e saúde do trabalho (EPIs, exames médicos periódicos, PCMSO e PGR)", "2", "2.2", null),
                ("2.2.6", "Capacitação, treinamentos técnicos e programas de desenvolvimento de pessoal", "2", "2.2", null),

                ("2.3.1", "Combustíveis e reagentes (óleo diesel S10, diesel S500, gasolina, etanol e Arla 32)", "2", "2.3", null),
                ("2.3.2", "Lubrificantes, filtros, aditivos e fluidos hidráulicos/transmissão", "2", "2.3", null),
                ("2.3.3", "Peças de reposição e componentes de desgaste (rolamentos, correias, bicos, discos e facas)", "2", "2.3", null),
                ("2.3.4", "Serviços mecânicos e oficinas externas (horas técnicas de concessionárias, solda e torno mecânico)", "2", "2.3", null),
                ("2.3.5", "Pneus, câmaras de ar e serviços de borracharia, recapagem e vulcanização", "2", "2.3", null),
                ("2.3.6", "Documentação, seguros da frota e telemetria (IPVA, licenciamento, apólices e rastreamento GPS)", "2", "2.3", null),
                ("2.3.7", "Locação operacional de veículos leves e máquinas de apoio", "2", "2.3", null),
                ("2.3.8", "Gestão ambiental da oficina (coleta de óleos lubrificantes usados/OLUC, baterias e filtros)", "2", "2.3", null),

                ("2.4.1", "Estradas internas, pontes de madeira/concreto, mata-burros, bueiros e curvas de nível", "2", "2.4", null),
                ("2.4.2", "Cercas perimetrais existentes, porteiras e corredores de movimentação", "2", "2.4", null),
                ("2.4.3", "Manutenção civil de edificações (alojamentos, casas de colonos, refeitórios, oficinas e tulhas)", "2", "2.4", null),
                ("2.4.4", "Redes elétricas rurais, transformadores, cabeamentos e manutenção de geradores a diesel", "2", "2.4", null),
                ("2.4.5", "Poços artesianos, bombas d'água de abastecimento geral, caixas d'água e tubulações", "2", "2.4", null),
                ("2.4.6", "Silos, moegas, secadores de grãos e armazéns próprios da propriedade", "2", "2.4", null),
                ("2.4.7", "Conservação de pátios, limpeza de áreas comuns e serviços de controle de pragas (desratização)", "2", "2.4", null),
                ("2.4.8", "Esgotamento sanitário e resíduos prediais (limpeza de fossas sépticas e sumidouros)", "2", "2.4", null),

                ("3.1.1", "Aquisição de máquinas pesadas e autopropelidos (tratores agrícolas, colheitadeiras e pulverizadores)", "3", "3.1", null),
                ("3.1.2", "Aquisição de implementos agrícolas (plantadeiras, grades aradoras/niveladoras e carretas graneleiras)", "3", "3.1", null),
                ("3.1.3", "Aquisição de veículos utilitários e transporte (camionetes 4x4, caminhões boiadeiros e motocicletas)", "3", "3.1", null),
                ("3.1.4", "Aquisição de tecnologia fixa e automação (antenas RTK base, sistemas de piloto automático e monitores)", "3", "3.1", null),

                ("3.2.1", "Construção civil de grande porte (barracões de máquinas, complexos de armazenagem e alojamentos)", "3", "3.2", null),
                ("3.2.2", "Infraestrutura de irrigação e recursos hídricos (novos pivôs centrais, represas e adutoras)", "3", "3.2", null),
                ("3.2.3", "Usinas de micro/minigeração de energia solar fotovoltaica e subestações elétricas", "3", "3.2", null),
                ("3.2.4", "Correção inicial profunda de solo em áreas de abertura (calagem e gessagem pesada de primeiro ano)", "3", "3.2", null),
                ("3.2.5", "Sistematização de terrenos e redes estruturais de drenagem de solo", "3", "3.2", null),

                ("3.3.1", "Infraestrutura e instalações pecuárias (currais antiestresse, bretes, balanças de fluxo e confinamento)", "3", "3.3", null),
                ("3.3.2", "Malha hídrica definitiva para pastejo (bebedouros de concreto e reservatórios metálicos australianos)", "3", "3.3", null),
                ("3.3.3", "Implantação de cercas definitivas novas e corredores mestres de manejo", "3", "3.3", null),
                ("3.3.4", "Investimentos em bovinos para plantel permanente (matrizes puras, reprodutores PO e receptoras)", "3", "3.3", null),
                ("3.3.5", "Formação e abertura de pastagens perenes (operações pesadas de preparo de solo e semeadura)", "3", "3.3", null),

                ("4.1.1", "Insumos de safras passadas (quitação de boletos e duplicatas de adubos e defensivos anteriores)", "4", "4.1", null),
                ("4.1.2", "Serviços e operações de safras passadas (fretes residuais, armazenagem de terceiros e colheitas antigas)", "4", "4.1", null),
                ("4.1.3", "Arrendamentos vencidos ou parcelamentos de safras pretéritas", "4", "4.1", null),

                ("4.2.1", "Renegociações bancárias e alongamentos formais de crédito rural (operações MCR, PESA e Securitização)", "4", "4.2", null),
                ("4.2.2", "Confissões de dívida com revendas e tradings (acordos estruturados de CPRs financeiras e washout)", "4", "4.2", null),
                ("4.2.3", "Parcelamentos fiscais, tributários e previdenciários (programas de Refis federal e passivos de Funrural)", "4", "4.2", null),
                ("4.2.4", "Acordos judiciais e homologações trabalhistas de exercícios anteriores", "4", "4.2", null),

                ("5.1.1", "Amortização de financiamentos e empréstimos bancários (valor principal das parcelas de investimento)", "5", "5.1", null),
                ("5.1.2", "Retiradas de pró-labore institucional dos proprietários e distribuição de lucros aos sócios", "5", "5.1", null),
                ("5.1.3", "Despesas pessoais e familiares (expurgo financeiro de contas particulares pagas via conta operacional)", "5", "5.1", null),
                ("5.1.4", "Tributos sobre patrimônio, doações e ganhos de capital (ITCMD, escrituração fundiária e inventários)", "5", "5.1", null),
                ("5.1.5", "Doações particulares, despesas filantrópicas e patrocínios não institucionais", "5", "5.1", null),

                ("6.1.1", "Aportes e integralização de capital em cooperativas agropecuárias, holdings e empresas coligadas", "6", "6.1", null),
                ("6.1.2", "Aplicações e ativos financeiros de longo prazo (títulos públicos, papéis do agronegócio e fundos)", "6", "6.1", null),
                ("6.1.3", "Aquisição de imóveis urbanos, galpões logísticos e terrenos para renda ou especulação", "6", "6.1", null),
                ("6.1.4", "Aquisição de novas propriedades rurais e terras brutas para expansão fundiária futura", "6", "6.1", null),
                ("6.1.5", "Concessão de mútuos e empréstimos financeiros entre empresas do grupo ou partes relacionadas", "6", "6.1", null)
            };

            foreach (var cc in costCenters)
            {
                var filter = Builders<CostCenter>.Filter.Eq(x => x.Code, cc.Code);
                var update = Builders<CostCenter>.Update
                    .Set(x => x.Name, cc.Name)
                    .Set(x => x.GroupCode, cc.GroupCode)
                    .Set(x => x.SubGroupCode, cc.SubGroupCode)
                    .Set(x => x.SubCostCenter, cc.SubCostCenter ?? "")
                    .Set(x => x.Deleted, false)
                    .SetOnInsert(x => x.Value, 0m)
                    .SetOnInsert(x => x.CreatedAt, now)
                    .SetOnInsert(x => x.CreatedBy, "system_seed");

                await db.CostCenters.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
            }
        }
    }
}
