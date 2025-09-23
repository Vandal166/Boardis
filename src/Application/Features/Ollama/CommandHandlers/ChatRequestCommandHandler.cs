using System.Net.Http.Json;
using System.Text.Json;
using Application.Abstractions.CQRS;
using Application.Contracts.Board;
using Application.DTOs.Ollama;
using Application.Features.Ollama.Commands;
using FluentResults;

namespace Application.Features.Ollama.CommandHandlers;

internal sealed class ChatRequestCommandHandler : ICommandHandler<ChatRequestCommand, ChatResponse>
{
    private readonly IBoardRepository _boardRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string PromptTemplate = @"
            You are an AI assistant helping users manage their Kanban boards. Use the following board context to inform your responses.

            Board Context:
            {0}

            Guidelines:
            - Provide clear, concise, and relevant answers based on the board context.
            - If the message is a question about the board, answer directly using the provided information.
            - If the message is a request for suggestions or next steps, analyze the board context and offer actionable advice.
            - Maintain a professional but friendly tone 😉.
            - If the message is unclear or lacks context, ask for clarification.

            Respond appropriately to help the user effectively manage their Kanban board.
            ";
    
    
    public ChatRequestCommandHandler(IBoardRepository boardRepository, IHttpClientFactory httpClientFactory)
    {
        _boardRepository = boardRepository;
        _httpClientFactory = httpClientFactory;
    }
    private const string OllamaApiUrl = "http://ollama:11434/api/chat";  // Internal container address
    private const string Model = "gemma2:2b";
    
    // the AI model is gonna be using the board context to answer questions about it, or make suggestions etc.
    // e.g. "Summarize the current state of the board" -> the model can generate a summary based on the board content
    public async Task<Result<ChatResponse>> Handle(ChatRequestCommand command, CancellationToken ct = default)
    {
        var board =  await _boardRepository.GetWithCards(command.BoardId, ct);
        if (board is null)
            return Result.Fail("BoardNotFound");
       
        // the full context of the board to provide to the model
        var boardContext = new
        {
            board.Title,
            board.Description,
            Lists = board.Lists
                .Select(l => new
                {
                    l.Title,
                    Cards = l.Cards
                        .Where(c => !string.IsNullOrWhiteSpace(c.Title.ToString()) || !string.IsNullOrWhiteSpace(c.Description))
                        .Select(c => new
                        {
                            c.Title,
                            Description = string.IsNullOrWhiteSpace(c.Description) ? null : c.Description,
                            c.CompletedAt
                        })
                        .ToList()
                })
                .Where(l => l.Cards.Count != 0)
                .ToList()
        };
       
        var fullPrompt = string.Format(PromptTemplate, JsonSerializer.Serialize(boardContext));
         
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
                messages = new[]
                {
                    new { role = "system", content = fullPrompt },
                    new { role = "user", content = command.Message }
                },
                options = new { temperature = 0.7 },
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
            Console.WriteLine($"Json response: {json.RootElement}");
            if (!json.RootElement.TryGetProperty("message", out var messageObj) ||
                !messageObj.TryGetProperty("content", out var resp))
            {
                Console.WriteLine("Ollama API response missing 'response' field.");
                return Result.Fail("OllamaApiResponseMissingResponseField");
            }
          

            var responseText = resp.ToString().Trim();  // Trim any whitespace
            Console.WriteLine($"Raw Ollama response: {responseText}");
             
            var chatResponse = new ChatResponse
            {
                ResponseMessage = responseText
            };

            Console.WriteLine("Ollama response successfully parsed to ChatResponse.");
            return Result.Ok(chatResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return Result.Fail("OllamaApiUnexpectedError");
        }
    }
}