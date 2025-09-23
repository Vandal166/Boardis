using System.Net.Http.Json;
using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.DTOs.Ollama;
using Application.Features.Ollama.Commands;
using FluentResults;

namespace Application.Features.Ollama.CommandHandlers;

internal sealed class GenerateListStructureCommandHandler : ICommandHandler<GenerateListStructureCommand, ListStructureResponse>
{
    private readonly IHttpClientFactory _httpClientFactory;

    private const string PromptTemplate = @"
            Generate Kanban lists and optional cards based on the user description: '{0}'.

            Rules:
            - Generate one or more lists as appropriate. If the description implies a single category, use one list with cards; otherwise, break into logical separate lists.
            - List 'name' must be a non-empty string (required).
            - If cards are included in a list, each card must have a non-empty 'title' string (required) and optional 'description'.
            - If no cards for a list, use an empty array [] for 'cards'.
            - Output ONLY valid JSON – no extra text, explanations, markdown, or incomplete structures.

            Exact format:
            {{
              ""lists"": [
                {{
                  ""name"": ""Required List Name"",
                  ""cards"": [
                    {{
                      ""title"": ""Required Card Title"",
                      ""description"": ""Optional description""
                    }}
                  ]
                }}
              ]
            }}

            EXAMPLES:
            Description: 'Organize a Kanban board for a website launch with Design, Development, and Testing lists. Add tasks for creating wireframes, coding the homepage, and running usability tests.'
            Output: {{
              ""lists"": [
                {{
                  ""name"": ""Design"",
                  ""cards"": [
                    {{ ""title"": ""Create Wireframes"", ""description"": ""Design wireframes for all main pages."" }},
                    {{ ""title"": ""Choose Color Scheme"", ""description"": ""Select a color palette for the website."" }}
                  ]
                }},
                {{
                  ""name"": ""Development"",
                  ""cards"": [
                    {{ ""title"": ""Code Homepage"", ""description"": ""Implement the homepage layout and navigation."" }},
                    {{ ""title"": ""Set Up Backend"", ""description"": ""Configure server and database for the site."" }}
                  ]
                }},
                {{
                  ""name"": ""Testing"",
                  ""cards"": [
                    {{ ""title"": ""Run Usability Tests"", ""description"": ""Test the site with real users for feedback."" }},
                    {{ ""title"": ""Fix Bugs"", ""description"": ""Resolve issues found during testing."" }}
                  ]
                }}
              ]
            }}

            Description: 'Plan a team event with lists for Preparation, Activities, and Follow-up. Add cards for booking a venue, organizing games, and sending thank-you notes.'
            Output: {{
              ""lists"": [
                {{
                  ""name"": ""Preparation"",
                  ""cards"": [
                    {{ ""title"": ""Book Venue"", ""description"": ""Reserve a location for the event."" }},
                    {{ ""title"": ""Arrange Catering"", ""description"": ""Order food and drinks for attendees."" }}
                  ]
                }},
                {{
                  ""name"": ""Activities"",
                  ""cards"": [
                    {{ ""title"": ""Organize Games"", ""description"": ""Plan team-building games and activities."" }},
                    {{ ""title"": ""Prepare Prizes"", ""description"": ""Get prizes for game winners."" }}
                  ]
                }},
                {{
                  ""name"": ""Follow-up"",
                  ""cards"": [
                    {{ ""title"": ""Send Thank-You Notes"", ""description"": ""Email appreciation messages to participants."" }},
                    {{ ""title"": ""Collect Feedback"", ""description"": ""Ask for suggestions to improve future events."" }}
                  ]
                }}
              ]
            }}
            END OF EXAMPLES.

            Ensure JSON is complete, parsable, and follows rules.";

    public GenerateListStructureCommandHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private const string OllamaApiUrl = "http://ollama:11434/api/generate";  // Internal container address
    private const string Model = "gemma2:2b";
    
    public async Task<Result<ListStructureResponse>> Handle(GenerateListStructureCommand command, CancellationToken ct = default)
    {
        var fullPrompt = string.Format(PromptTemplate, command.Description);
        
        try
        {
            Console.WriteLine("Starting Ollama API request...");
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2);
            Console.WriteLine($"Using model: {Model}");
            Console.WriteLine($"Full prompt: {fullPrompt}");

            var requestBody = new
            {
                model = Model,
                prompt = fullPrompt,
                options = new { temperature = 0.2 },  // low temperature for more deterministic, structured output
                stream = false
            };

            Console.WriteLine("Sending POST request to Ollama API...");
            var response = await httpClient.PostAsJsonAsync(OllamaApiUrl, requestBody, ct);
            Console.WriteLine($"Received response with status code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Ollama API request failed with status code: {response.StatusCode}");
                return Result.Fail("OllamaApiRequestFailedWithStatusCode");
            }

            var content = await response.Content.ReadAsStringAsync(ct);
             
            var json = JsonDocument.Parse(content);
            if (!json.RootElement.TryGetProperty("response", out var resp))
            {
                Console.WriteLine("Ollama API response missing 'response' field.");
                return Result.Fail("OllamaApiResponseMissingResponseField");
            }
          

            var responseText = resp.ToString().Trim();  // Trim any whitespace
            Console.WriteLine($"Raw Ollama response: {responseText}");
            // Remove code block markers if present
            var cleanedResponse = responseText
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();

            // Attempt to deserialize to strongly typed model
            ListStructureResponse? structureResponse;
            try
            {
                structureResponse = JsonSerializer.Deserialize<ListStructureResponse>(cleanedResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true  // Handle case variations if model hallucinates
                });
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization failed: {ex.Message}");
                return Result.Fail("InvalidJsonStructureFromAi");
            }

            if (structureResponse is null)
            {
                return Result.Fail("AiResponseEmptyOrNull");
            }

            Console.WriteLine("Ollama response successfully parsed to Kanban structure.");
            return Result.Ok(structureResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return Result.Fail("OllamaApiUnexpectedError");
        }
    }
}