using System;
using System.Reflection;
using System.Text;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Centraliza todos os prompts utilizados pelo AgentService
    /// </summary>
    public static class AgentPrompts
    {
        public static string BuildIdentificationPrompt(string apiDocumentation, string message)
        {
            return $@"Você é um especialista em APIs REST que identifica qual endpoint deve ser chamado baseado na intenção do usuário.

Documentação Completa da API:
{apiDocumentation}

Mensagem do usuário: {message}

INSTRUÇÕES:
1. Analise a mensagem do usuário e identifique a intenção (listar, criar, atualizar, deletar, buscar, etc.)
2. Encontre o controller e action que melhor corresponde à intenção
3. Preste atenção especial à DESCRIÇÃO de cada endpoint
4. Para buscas/listagens, use endpoints GET ou POST com 'Filter', 'Search', 'GetBy', etc.
5. Para criação, use endpoints POST com 'Save', 'Create', 'Add', etc.

EXEMPLOS:
- 'Liste todos os evento criados' => Evento.Get
- 'Criar um novo Evento' => Evento.Save (porque cria evento)
- 'Obter detalhes do Evento 123' => Evento.Get(id) (porque obtem um Evento especifico)
- 'Atualizar local do Evento' => Evento.Put(int id, EventoDto model) (porque atualiza o evento)

Responda APENAS com 'ControllerName.ActionName' (sem Controller no nome).
Exemplo de resposta: Evento.Get";
        }

        public static string BuildExecutionAnalysisPrompt(string message)
        {
            return $@"Você é um especialista em analisar comandos complexos e determinar se requerem múltiplas etapas.

IMPORTANTE: Use APENAS os métodos que existem na API. Nunca invente métodos.

MÉTODOS DISPONÍVEIS NO EventoController:
- Get(int id): Busca um Evento pelo ID
- Post(EventoDto model): Salva um novo Evento
- Put(int id, EventoDto model): Edita um Evento existente
- Get(): Busca todos eventos(Não suporta limite 'top X' ou 'first X' nativamente)
- Delete(int id): Deleta um Evento

COMANDOS QUE REQUEREM MÚLTIPLAS ETAPAS (isMultiStep: true):
1. OPERAÇÕES DE CÓPIA/CLONE:
   - Comandos com ""copiar/clonar"" seguido de ""modificar/alterar"" seguido de ""salvar""

2. LISTAGENS COM LIMITES OU FILTROS ESPECIAIS (MUITO IMPORTANTE):
   - Comandos que pedem ""X primeiros"", ""Top X"", ""Último"", ""Apenas 1""
   - A API retorna TODOS os resultados. Você DEVE adicionar um passo de transformação para filtrar a quantidade.
   - Exemplo: ""3 primeiros evento"" -> Requer passo de transformação.

3. OPERAÇÕES SEQUENCIAIS:
   - Comandos com conectores como ""e depois"", ""em seguida"", ""então""

COMO PLANEJAR:
- Se o usuário pedir ""Liste os 3 primeiros eventos"":
  - Passo 1: Get(para buscar todos os evento do período)
  - Passo 2: TRANSFORM_DATA (para selecionar apenas os 3 primeiros da lista retornada)

EXEMPLOS DE MULTI-ETAPA:

- 'Liste os 3 primeiros evento deste mês'
  - Etapa 1: {{""order"": 1, ""controller"": ""Evento"", ""action"": ""Get"", ""parameterSource"": ""user_input"", ""description"": ""Buscar todos os evento deste mês""}}
  - Etapa 2: {{""order"": 2, ""controller"": ""Evento"", ""action"": ""FILTER_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""A partir da lista anterior filtre aqueles do mês corrente"", ""transformation"": ""select_month""}}
- Etapa 3: {{""order"": 3, ""controller"": ""Evento"", ""action"": ""LIMIT_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""Limitar a lista aos 3 primeiros evento"", ""transformation"": ""take_first_3""}}  

- 'Copie o Evento 1907 adicione 30 dias na data do evento e depois salve'
  - Etapa 1: {{""order"": 1, ""controller"": ""Evento"", ""action"": ""Get"", ""parameterSource"": ""user_input"", ""description"": ""Buscar Evento 1907""}}
  - Etapa 2: {{""order"": 2, ""controller"": ""Evento"", ""action"": ""TRANSFORM_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""Adicionar 30 dias na data do evento"", ""transformation"": ""add_30_days""}}
  - Etapa 3: {{""order"": 3, ""controller"": ""Evento"", ""action"": ""Post"", ""parameterSource"": ""previous_step"", ""description"": ""Salvar o novo Evento com a data modificada""}}

EXEMPLOS DE ETAPA ÚNICA (isMultiStep: false):
- ""Liste todos os eventos"" (Sem limite de quantidade)
- ""Busque o Evento 1907""
- ""Delete o Evento 123""

MENSAGEM DO USUÁRIO:
{message}

Responda em JSON com esta estrutura:
{{
  ""isMultiStep"": true/false,
  ""reasoning"": ""explicação do motivo da decisão"",
  ""steps"": [
    {{
      ""order"": 1,
      ""description"": ""descrição do passo"",
      ""controller"": ""NomeController"",
      ""action"": ""NomeAction"" ou ""TRANSFORM_DATA"" para processamento interno,
      ""parameterSource"": ""user_input"" ou ""previous_step"" ou ""computed"",
      ""transformation"": ""tipo de transformação se action for TRANSFORM_DATA""
    }}
  ]
}}

Se for etapa única, retorne isMultiStep: false e steps vazio.";
        }

        public static string BuildExtractionPrompt(
            string message,
            string controllerName,
            string actionName,
            ParameterInfo[] parameters
        )
        {
            var sb = new StringBuilder();
            var currentDate = DateTime.Now;

            // Calcular diferentes contextos temporais
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfMonth.AddDays(-1);
            var firstDayOfYear = new DateTime(currentDate.Year, 1, 1);
            var lastDayOfYear = new DateTime(currentDate.Year, 12, 31);
            var firstDayOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            var lastDayOfWeek = firstDayOfWeek.AddDays(6);

            sb.AppendLine("Você é um assistente especializado em preencher parâmetros de APIs.");
            sb.AppendLine();
            sb.AppendLine("## CONTEXTO TEMPORAL");
            sb.AppendLine($"- Data/hora atual: {currentDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- Hoje: {currentDate:yyyy-MM-dd}");
            sb.AppendLine(
                $"- Este mês: {firstDayOfMonth:yyyy-MM-dd} a {lastDayOfMonth:yyyy-MM-dd}"
            );
            sb.AppendLine(
                $"- Mês passado: {firstDayOfLastMonth:yyyy-MM-dd} a {lastDayOfLastMonth:yyyy-MM-dd}"
            );
            sb.AppendLine(
                $"- Esta semana: {firstDayOfWeek:yyyy-MM-dd} a {lastDayOfWeek:yyyy-MM-dd}"
            );
            sb.AppendLine($"- Este ano: {firstDayOfYear:yyyy-MM-dd} a {lastDayOfYear:yyyy-MM-dd}");
            sb.AppendLine();

            sb.AppendLine($"## MENSAGEM DO USUÁRIO");
            sb.AppendLine(message);
            sb.AppendLine();

            sb.AppendLine($"## MÉTODO A SER CHAMADO");
            sb.AppendLine($"{controllerName}.{actionName}");
            sb.AppendLine();

            sb.AppendLine("## PARÂMETROS NECESSÁRIOS");
            sb.AppendLine(ParameterInfoBuilder.BuildParameterInfo(parameters));
            sb.AppendLine();

            sb.AppendLine("## INSTRUÇÕES DE PREENCHIMENTO");
            sb.AppendLine();

            var param = parameters.FirstOrDefault();
            if (param != null)
            {
                sb.Append(
                    BuildParameterInstructions(param, firstDayOfMonth, lastDayOfMonth, currentDate)
                );
            }

            sb.AppendLine();
            sb.AppendLine("## RESPOSTA ESPERADA");
            sb.AppendLine("- Retorne APENAS o JSON válido com os parâmetros");
            sb.AppendLine("- NÃO adicione explicações, comentários ou texto adicional");
            sb.AppendLine("- Use null para valores não especificados");
            sb.AppendLine("- Seja preciso na interpretação de termos temporais");

            return sb.ToString();
        }

        public static string BuildDataTransformationPrompt(
            string dataToTransform,
            string transformation,
            string description,
            string userMessage
        )
        {
            return $@"Você é um especialista em analisar e transformar dados de evento para atender exatamente à solicitação do usuário.

MENSAGEM ORIGINAL DO USUÁRIO:
""{userMessage}""

DADOS DO PASSO ANTERIOR (Resultado da busca/operação anterior):
{dataToTransform}

TRANSFORMAÇÃO TÉCNICA SUGERIDA: {transformation}
DESCRIÇÃO TÉCNICA: {description}

OBJETIVO:
Analise a MENSAGEM ORIGINAL DO USUÁRIO e verifique se os DADOS DO PASSO ANTERIOR atendem ao que foi Evento.
Se necessário, filtre, ordene ou transforme os dados para corresponder EXATAMENTE ao que o usuário pediu.

TIPOS DE OPERAÇÃO:

1. FILTRO/SELEÇÃO (ex: ""3 primeiros"", ""último"", ""do palestrante X""):
   - Se o usuário pediu ""X primeiros"", retorne apenas os X primeiros itens do array.
   - Se o usuário pediu ""último"", retorne apenas o último.
   - Se o usuário pediu um filtro específico não aplicado anteriormente, aplique-o agora.
   - Preserve a estrutura dos objetos.

2. MODIFICAÇÃO (ex: ""altere o valor"", ""copie e mude X""):
   - Se for uma modificação, aplique as regras de transformação (add_days, set_value, etc.).
   - Mantenha os campos obrigatórios.

INSTRUÇÕES CRÍTICAS PARA FILTROS (Arrays):
- Retorne um ARRAY JSON válido: [{{...}}, {{...}}]
- Se o usuário pediu ""3 primeiros"", o array DEVE ter no máximo 3 elementos.
- Se a lista de entrada estiver vazia, retorne array vazio [].
- NÃO retorne apenas os IDs, retorne os objetos completos.

INSTRUÇÕES CRÍTICAS PARA MODIFICAÇÕES (Objetos):
- Retorne o objeto modificado.
- Remova campos de sistema (id, dhCriacao, rPs, etc.) se for para criar novo.
- Mantenha campos obrigatórios (nomeConfiguracaoProduto, etc.).

EXEMPLOS:

Exemplo 1 - ""Liste os 3 primeiros evento""
Entrada: [A, B, C, D, E]
Saída: [A, B, C] (Apenas os 3 primeiros)

Exemplo 2 - ""Pegue o último Evento e mude o palestrante para Globo""
Entrada: [A, B, C]
Saída: {{...C, palestrante: ""Globo""...}} (Objeto modificado)

RETORNE APENAS O JSON VÁLIDO COM O RESULTADO FINAL (ARRAY OU OBJETO). SEM EXPLICAÇÕES.";
        }

        public static string BuildParameterMappingPrompt(
            string previousDataJson,
            string description,
            string targetTypeName,
            string typePropertiesDescription
        )
        {
            return $@"Você é um especialista em transformar dados de API.

DADOS DO PASSO ANTERIOR:
{previousDataJson}

PASSO ATUAL: {description}

TIPO DE PARÂMETRO NECESSÁRIO: {targetTypeName}

PROPRIEDADES DO TIPO:
{typePropertiesDescription}

INSTRUÇÕES CRÍTICAS:
1. Extraia o objeto 'data' dos dados anteriores se existir
2. Mantenha TODOS os campos obrigatórios com seus valores originais
3. Para EventoNewModel:
   - COPIE EXATAMENTE: Lotes, RedesSociais, PalestrantesEventos
   - COPIE: Local, Tema, Telefone, Email e outros campos simples
   4. Remova APENAS campos de sistema: id
   5. Mantenha valores null, vazios ou zero conforme estão no original

⚠️ CRÍTICO: NÃO transforme o objeto em valores padrão/zerados. PRESERVE os dados originais!

Retorne APENAS o JSON válido com o objeto transformado:";
        }

        private static string BuildParameterInstructions(
            ParameterInfo param,
            DateTime firstDayOfMonth,
            DateTime lastDayOfMonth,
            DateTime currentDate
        )
        {
            var sb = new StringBuilder();
            var paramType = param.ParameterType;

            if (paramType == typeof(int) || paramType == typeof(int?))
            {
                sb.AppendLine("IMPORTANTE: O parâmetro esperado é um número inteiro.");
                sb.AppendLine(
                    "- Se a mensagem mencionar 'Evento X', 'id X', 'código X', extraia o número"
                );
                sb.AppendLine("- Retorne apenas o número (ex: 12345), não um objeto JSON");
                sb.AppendLine();
                sb.AppendLine($"Exemplo para '{param.Name}': 42");
            }
            else if (paramType == typeof(string))
            {
                sb.AppendLine("IMPORTANTE: O parâmetro esperado é uma string.");
                sb.AppendLine("- Extraia o texto mencionado na mensagem");
                sb.AppendLine("- Retorne apenas a string, sem aspas extras");
                sb.AppendLine();
                sb.AppendLine($"Exemplo para '{param.Name}': Magazine Luiza");
            }
            else if (
                paramType.IsGenericType
                && paramType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)
            )
            {
                var innerType = paramType.GetGenericArguments()[0];
                sb.AppendLine($"IMPORTANTE: O parâmetro esperado é uma lista de {innerType.Name}.");
                sb.AppendLine("- Retorne um array JSON");
                sb.AppendLine();
                sb.AppendLine(
                    $"Exemplo: [{(innerType == typeof(int) ? "1, 2, 3" : "\"item1\", \"item2\"")}]"
                );
            }
            else if (paramType.IsClass)
            {
                sb.AppendLine("IMPORTANTE: O parâmetro esperado é um objeto complexo.");
                sb.AppendLine();
                sb.AppendLine("### Regras para preenchimento:");
                sb.AppendLine("1. **Interpretação Temporal:**");
                sb.AppendLine(
                    "   - 'este mês' / 'mês atual' → DataEvento = primeiro dia do mês, DataEvento = último dia do mês"
                );
                sb.AppendLine("   - 'hoje' → data atual");
                sb.AppendLine("   - 'esta semana' → primeiro e último dia da semana");
                sb.AppendLine(
                    "   - 'mês passado' / 'último mês' → primeiro e último dia do mês anterior"
                );
                sb.AppendLine("   - 'este ano' → 01/01 até 31/12 do ano atual");
                sb.AppendLine();
                sb.AppendLine("2. **Datas:**");
                sb.AppendLine("   - Formato ISO 8601: 'yyyy-MM-ddTHH:mm:ss'");
                sb.AppendLine("   - Início do dia: 'T00:00:00'");
                sb.AppendLine("   - Fim do dia: 'T23:59:59'");
                sb.AppendLine();
                sb.AppendLine("3. **Valores não mencionados:**");
                sb.AppendLine("   - Use null para propriedades não especificadas");
                sb.AppendLine("   - Não use valores padrão (0, \"\", false)");
                sb.AppendLine();
                sb.AppendLine("4. **Strings:**");
                sb.AppendLine("   - Copie exatamente como aparecem na mensagem");
                sb.AppendLine("   - Mantenha maiúsculas/minúsculas");
                sb.AppendLine();

                sb.Append(
                    BuildParameterExamples(
                        param,
                        paramType,
                        firstDayOfMonth,
                        lastDayOfMonth,
                        currentDate
                    )
                );
            }

            return sb.ToString();
        }

        private static string BuildParameterExamples(
            ParameterInfo param,
            Type paramType,
            DateTime firstDayOfMonth,
            DateTime lastDayOfMonth,
            DateTime currentDate
        )
        {
            var sb = new StringBuilder();
            sb.AppendLine("### Exemplos:");

            if (paramType.Name.Contains("Filter"))
            {
                sb.AppendLine();
                sb.AppendLine($"Exemplo 1 - 'Liste os evento criados este mês':");
                sb.AppendLine("{");
                sb.AppendLine($"  \"{param.Name}\": {{");
                sb.AppendLine(
                    $"    \"DataInicioCriacao\": \"{firstDayOfMonth:yyyy-MM-dd}T00:00:00\","
                );
                sb.AppendLine(
                    $"    \"DataTerminoCriacao\": \"{lastDayOfMonth:yyyy-MM-dd}T23:59:59\""
                );
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine($"Exemplo 2 - 'evento do palestrante Magazine Luiza criados hoje':");
                sb.AppendLine("{");
                sb.AppendLine($"  \"{param.Name}\": {{");
                sb.AppendLine("    \"Mnepalestrante\": \"Magazine Luiza\",");
                sb.AppendLine($"    \"DataInicioCriacao\": \"{currentDate:yyyy-MM-dd}T00:00:00\",");
                sb.AppendLine($"    \"DataTerminoCriacao\": \"{currentDate:yyyy-MM-dd}T23:59:59\"");
                sb.AppendLine("  }");
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"Estrutura esperada:");
                sb.AppendLine("{");
                sb.AppendLine($"  \"{param.Name}\": {{");

                var props = paramType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props.Take(5))
                {
                    var exampleValue = TypeHelper.GetExampleValue(prop.PropertyType);
                    sb.AppendLine($"    \"{prop.Name}\": {exampleValue},");
                }
                if (props.Length > 5)
                {
                    sb.AppendLine("    // ... outras propriedades");
                }

                sb.AppendLine("  }");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
