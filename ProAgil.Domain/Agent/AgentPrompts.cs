using System;
using System.Reflection;
using System.Text;

namespace ProAgil.Domain.Agent
{
    /// <summary>
    /// Centralizes all prompts used by the AgentService
    /// </summary>
    public static class AgentPrompts
    {
        public static string BuildIdentificationPrompt(string apiDocumentation, string message)
        {
            return $@"You are an expert in REST APIs that identifies which endpoint should be called based on user intent.

Complete API Documentation:
{apiDocumentation}

User message: {message}

INSTRUCTIONS:
1. Analyze the user message and identify the intent (list, create, update, delete, search, etc.)
2. Find the controller and action that best matches the intent
3. Pay special attention to the DESCRIPTION of each endpoint
4. For searches/listings, use GET or POST endpoints with 'Filter', 'Search', 'GetBy', etc.
5. For creation, use POST endpoints with 'Save', 'Create', 'Add', etc.

EXAMPLES:
- 'List all created events' => Event.Get
- 'Create a new Event' => Event.Save (because it creates an event)
- 'Get details of Event 123' => Event.Get(id) (because it gets a specific Event)
- 'Update Event location' => Event.Put(int id, EventDto model) (because it updates the event)

Respond ONLY with 'ControllerName.ActionName' (without Controller in the name).
Example response: Event.Get";
        }

        public static string BuildExecutionAnalysisPrompt(string message)
        {
            return $@"You are an expert in analyzing complex commands and determining if they require multiple steps.

IMPORTANT: Use ONLY methods that exist in the API. Never invent methods.

AVAILABLE METHODS IN EventController:
- Get(int id): Search for an Event by ID
- Post(EventDto model): Save a new Event
- Put(int id, EventDto model): Edit an existing Event
- Get(): Get all events (Does not natively support 'top X' or 'first X' limits)
- Delete(int id): Delete an Event

COMMANDS THAT REQUIRE MULTIPLE STEPS (isMultiStep: true):
1. COPY/CLONE OPERATIONS:
   - Commands with ""copy/clone"" followed by ""modify/change"" followed by ""save""

2. LISTINGS WITH LIMITS OR SPECIAL FILTERS (VERY IMPORTANT):
   - Commands that ask for ""first X"", ""Top X"", ""Last"", ""Only 1""
   - The API returns ALL results. You MUST add a transformation step to filter the quantity.
   - Example: ""first 3 events"" -> Requires transformation step.

3. SEQUENTIAL OPERATIONS:
   - Commands with connectors like ""and then"", ""next"", ""then""

HOW TO PLAN:
- If the user asks ""List the first 3 events"":
  - Step 1: Get (to fetch all events from the period)
  - Step 2: TRANSFORM_DATA (to select only the first 3 from the returned list)

MULTI-STEP EXAMPLES:

- 'List the first 3 events this month'
  - Step 1: {{""order"": 1, ""controller"": ""Event"", ""action"": ""Get"", ""parameterSource"": ""user_input"", ""description"": ""Fetch all events this month""}}
  - Step 2: {{""order"": 2, ""controller"": ""Event"", ""action"": ""FILTER_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""From the previous list filter those from the current month"", ""transformation"": ""select_month""}}
- Step 3: {{""order"": 3, ""controller"": ""Event"", ""action"": ""LIMIT_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""Limit the list to the first 3 events"", ""transformation"": ""take_first_3""}}  

- 'Copy Event 1907 add 30 days to the event date and then save'
  - Step 1: {{""order"": 1, ""controller"": ""Event"", ""action"": ""Get"", ""parameterSource"": ""user_input"", ""description"": ""Fetch Event 1907""}}
  - Step 2: {{""order"": 2, ""controller"": ""Event"", ""action"": ""TRANSFORM_DATA"", ""parameterSource"": ""previous_step"", ""description"": ""Add 30 days to the event date"", ""transformation"": ""add_30_days""}}
  - Step 3: {{""order"": 3, ""controller"": ""Event"", ""action"": ""Post"", ""parameterSource"": ""previous_step"", ""description"": ""Save the new Event with the modified date""}}

SINGLE STEP EXAMPLES (isMultiStep: false):
- ""List all events"" (No quantity limit)
- ""Search for Event 1907""
- ""Delete Event 123""

USER MESSAGE:
{message}

Respond in JSON with this structure:
{{
  ""isMultiStep"": true/false,
  ""reasoning"": ""explanation of the decision"",
  ""steps"": [
    {{
      ""order"": 1,
      ""description"": ""step description"",
      ""controller"": ""ControllerName"",
      ""action"": ""ActionName"" or ""TRANSFORM_DATA"" for internal processing,
      ""parameterSource"": ""user_input"" or ""previous_step"" or ""computed"",
      ""transformation"": ""transformation type if action is TRANSFORM_DATA""
    }}
  ]
}}

If single step, return isMultiStep: false and empty steps.";
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

            // Calculate different temporal contexts
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var firstDayOfLastMonth = firstDayOfMonth.AddMonths(-1);
            var lastDayOfLastMonth = firstDayOfMonth.AddDays(-1);
            var firstDayOfYear = new DateTime(currentDate.Year, 1, 1);
            var lastDayOfYear = new DateTime(currentDate.Year, 12, 31);
            var firstDayOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
            var lastDayOfWeek = firstDayOfWeek.AddDays(6);

            sb.AppendLine("You are an assistant specialized in filling API parameters.");
            sb.AppendLine();
            sb.AppendLine("## TEMPORAL CONTEXT");
            sb.AppendLine($"- Current date/time: {currentDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"- Today: {currentDate:yyyy-MM-dd}");
            sb.AppendLine(
                $"- This month: {firstDayOfMonth:yyyy-MM-dd} to {lastDayOfMonth:yyyy-MM-dd}"
            );
            sb.AppendLine(
                $"- Last month: {firstDayOfLastMonth:yyyy-MM-dd} to {lastDayOfLastMonth:yyyy-MM-dd}"
            );
            sb.AppendLine(
                $"- This week: {firstDayOfWeek:yyyy-MM-dd} to {lastDayOfWeek:yyyy-MM-dd}"
            );
            sb.AppendLine($"- This year: {firstDayOfYear:yyyy-MM-dd} to {lastDayOfYear:yyyy-MM-dd}");
            sb.AppendLine();

            sb.AppendLine($"## USER MESSAGE");
            sb.AppendLine(message);
            sb.AppendLine();

            sb.AppendLine($"## METHOD TO BE CALLED");
            sb.AppendLine($"{controllerName}.{actionName}");
            sb.AppendLine();

            sb.AppendLine("## REQUIRED PARAMETERS");
            sb.AppendLine(ParameterInfoBuilder.BuildParameterInfo(parameters));
            sb.AppendLine();

            sb.AppendLine("## FILLING INSTRUCTIONS");
            sb.AppendLine();

            var param = parameters.FirstOrDefault();
            if (param != null)
            {
                sb.Append(
                    BuildParameterInstructions(param, firstDayOfMonth, lastDayOfMonth, currentDate)
                );
            }

            sb.AppendLine();
            sb.AppendLine("## EXPECTED RESPONSE");
            sb.AppendLine("- Return ONLY valid JSON with parameters");
            sb.AppendLine("- DO NOT add explanations, comments, or additional text");
            sb.AppendLine("- Use null for unspecified values");
            sb.AppendLine("- Be precise in interpreting temporal terms");

            return sb.ToString();
        }

        public static string BuildDataTransformationPrompt(
            string dataToTransform,
            string transformation,
            string description,
            string userMessage
        )
        {
            return $@"You are an expert in analyzing and transforming event data to exactly meet the user's request.

ORIGINAL USER MESSAGE:
""{userMessage}""

PREVIOUS STEP DATA (Result of previous search/operation):
{dataToTransform}

SUGGESTED TECHNICAL TRANSFORMATION: {transformation}
TECHNICAL DESCRIPTION: {description}

OBJECTIVE:
Analyze the ORIGINAL USER MESSAGE and verify if the PREVIOUS STEP DATA meets what was requested.
If necessary, filter, sort, or transform the data to EXACTLY match what the user asked for.

OPERATION TYPES:

1. FILTER/SELECTION (e.g.: ""first 3"", ""last"", ""from speaker X""):
   - If the user asked for ""first X"", return only the first X items from the array.
   - If the user asked for ""last"", return only the last one.
   - If the user asked for a specific filter not previously applied, apply it now.
   - Preserve the object structure.

2. MODIFICATION (e.g.: ""change the value"", ""copy and change X""):
   - If it's a modification, apply the transformation rules (add_days, set_value, etc.).
   - Keep required fields.

CRITICAL INSTRUCTIONS FOR FILTERS (Arrays):
- Return a valid JSON ARRAY: [{{...}}, {{...}}]
- If the user asked for ""first 3"", the array MUST have at most 3 elements.
- If the input list is empty, return empty array [].
- DO NOT return only IDs, return complete objects.

CRITICAL INSTRUCTIONS FOR MODIFICATIONS (Objects):
- Return the modified object.
- Remove system fields (id, createdAt, rPs, etc.) if creating new.
- Keep required fields (productConfigurationName, etc.).

EXAMPLES:

Example 1 - ""List the first 3 events""
Input: [A, B, C, D, E]
Output: [A, B, C] (Only the first 3)

Example 2 - ""Get the last Event and change the speaker to Globe""
Input: [A, B, C]
Output: {{...C, speaker: ""Globe""...}} (Modified object)

RETURN ONLY VALID JSON WITH THE FINAL RESULT (ARRAY OR OBJECT). NO EXPLANATIONS.";
        }

        public static string BuildParameterMappingPrompt(
            string previousDataJson,
            string description,
            string targetTypeName,
            string typePropertiesDescription
        )
        {
            return $@"You are an expert in transforming API data.

PREVIOUS STEP DATA:
{previousDataJson}

CURRENT STEP: {description}

REQUIRED PARAMETER TYPE: {targetTypeName}

TYPE PROPERTIES:
{typePropertiesDescription}

CRITICAL INSTRUCTIONS:
1. Extract the 'data' object from previous data if it exists
2. Keep ALL required fields with their original values
3. For EventNewModel:
   - COPY EXACTLY: Batches, SocialNetworks, SpeakerEvents
   - COPY: Location, Theme, Phone, Email and other simple fields
   4. Remove ONLY system fields: id
   5. Keep null, empty, or zero values as they are in the original

⚠️ CRITICAL: DO NOT transform the object to default/zeroed values. PRESERVE original data!

Return ONLY valid JSON with the transformed object:";
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
                sb.AppendLine("IMPORTANT: The expected parameter is an integer.");
                sb.AppendLine(
                    "- If the message mentions 'Event X', 'id X', 'code X', extract the number"
                );
                sb.AppendLine("- Return only the number (e.g.: 12345), not a JSON object");
                sb.AppendLine();
                sb.AppendLine($"Example for '{param.Name}': 42");
            }
            else if (paramType == typeof(string))
            {
                sb.AppendLine("IMPORTANT: The expected parameter is a string.");
                sb.AppendLine("- Extract the text mentioned in the message");
                sb.AppendLine("- Return only the string, without extra quotes");
                sb.AppendLine();
                sb.AppendLine($"Example for '{param.Name}': Magazine Store");
            }
            else if (
                paramType.IsGenericType
                && paramType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>)
            )
            {
                var innerType = paramType.GetGenericArguments()[0];
                sb.AppendLine($"IMPORTANT: The expected parameter is a list of {innerType.Name}.");
                sb.AppendLine("- Return a JSON array");
                sb.AppendLine();
                sb.AppendLine(
                    $"Example: [{(innerType == typeof(int) ? "1, 2, 3" : "\"item1\", \"item2\"")}]"
                );
            }
            else if (paramType.IsClass)
            {
                sb.AppendLine("IMPORTANT: The expected parameter is a complex object.");
                sb.AppendLine();
                sb.AppendLine("### Filling rules:");
                sb.AppendLine("1. **Temporal Interpretation:**");
                sb.AppendLine(
                    "   - 'this month' / 'current month' → EventDate = first day of month, EventDate = last day of month"
                );
                sb.AppendLine("   - 'today' → current date");
                sb.AppendLine("   - 'this week' → first and last day of the week");
                sb.AppendLine(
                    "   - 'last month' / 'previous month' → first and last day of the previous month"
                );
                sb.AppendLine("   - 'this year' → 01/01 to 12/31 of current year");
                sb.AppendLine();
                sb.AppendLine("2. **Dates:**");
                sb.AppendLine("   - ISO 8601 format: 'yyyy-MM-ddTHH:mm:ss'");
                sb.AppendLine("   - Start of day: 'T00:00:00'");
                sb.AppendLine("   - End of day: 'T23:59:59'");
                sb.AppendLine();
                sb.AppendLine("3. **Unmentioned values:**");
                sb.AppendLine("   - Use null for unspecified properties");
                sb.AppendLine("   - Do not use default values (0, \"\", false)");
                sb.AppendLine();
                sb.AppendLine("4. **Strings:**");
                sb.AppendLine("   - Copy exactly as they appear in the message");
                sb.AppendLine("   - Keep uppercase/lowercase");
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
            sb.AppendLine("### Examples:");

            if (paramType.Name.Contains("Filter"))
            {
                sb.AppendLine();
                sb.AppendLine($"Example 1 - 'List events created this month':");
                sb.AppendLine("{");
                sb.AppendLine($"  \"{param.Name}\": {{");
                sb.AppendLine(
                    $"    \"CreationStartDate\": \"{firstDayOfMonth:yyyy-MM-dd}T00:00:00\","
                );
                sb.AppendLine(
                    $"    \"CreationEndDate\": \"{lastDayOfMonth:yyyy-MM-dd}T23:59:59\""
                );
                sb.AppendLine("  }");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine($"Example 2 - 'Events from speaker Magazine Store created today':");
                sb.AppendLine("{");
                sb.AppendLine($"  \"{param.Name}\": {{");
                sb.AppendLine("    \"SpeakerName\": \"Magazine Store\",");
                sb.AppendLine($"    \"CreationStartDate\": \"{currentDate:yyyy-MM-dd}T00:00:00\",");
                sb.AppendLine($"    \"CreationEndDate\": \"{currentDate:yyyy-MM-dd}T23:59:59\"");
                sb.AppendLine("  }");
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine($"Expected structure:");
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
                    sb.AppendLine("    // ... other properties");
                }

                sb.AppendLine("  }");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }
    }
}
