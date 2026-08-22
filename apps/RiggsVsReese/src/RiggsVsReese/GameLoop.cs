using RiggsVsReese.Services;

namespace RiggsVsReese;

internal sealed class GameLoop
{
    private readonly GameTurnService _turnService;

    internal GameLoop(GameTurnService turnService)
    {
        _turnService = turnService;
    }

    internal async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var response = await _turnService.StartAsync(cancellationToken);
        WriteResponse(response);

        while (ReadPlayerCommand() is { } command)
        {
            response = await _turnService.RespondAsync(command, cancellationToken);
            WriteResponse(response);
        }
    }

    private static string? ReadPlayerCommand()
    {
        Console.Write("> ");
        var command = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(command) ||
            command.Equals("/exit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return command;
    }

    private static void WriteResponse(string? response)
    {
        Console.WriteLine();
        Console.WriteLine(response);
        Console.WriteLine();
    }
}
