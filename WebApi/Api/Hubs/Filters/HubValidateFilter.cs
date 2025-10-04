using Microsoft.AspNetCore.SignalR;
using System.Reflection;
using WebApi.Api.Hubs.Attributes;
using WebApi.Application.Managers.Interfaces;
using WebApi.Domain.Entities;

public class HubValidateFilter : IHubFilter
{
    private readonly IGameManager _gameManager;
    private readonly IPlayerManager _playerManager;

    public HubValidateFilter(
        IGameManager gameManager,
        IPlayerManager playerManager)
    {
        _gameManager = gameManager;
        _playerManager = playerManager;
    }

    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        MethodInfo method = context.HubMethod;
        Dictionary<string, int> parameters = method.GetParameters()
            .Select((parameter, index) => new { parameter.Name, Index = index })
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.Ordinal);

        IReadOnlyList<object> arguments = context.HubMethodArguments;
        IEnumerable<ValidateHubArgumentAttribute> attributes = method.GetCustomAttributes<ValidateHubArgumentAttribute>();

        foreach (ValidateHubArgumentAttribute attr in attributes)
        {
            if (!parameters.TryGetValue(attr.ArgumentName, out int index) || index >= arguments.Count)
            {
                continue;
            }

            object argument = arguments[index];

            switch (attr.Type)
            {
                case ValidationType.GameHash:
                    ValidateGameHash(context, method.Name, argument);
                    break;

                case ValidationType.PlayerToken:
                    ValidatePlayerToken(context, method.Name, argument);
                    break;

                case ValidationType.DrawingToken:
                    ValidateDrawingToken(context, method.Name, argument);
                    break;

                case ValidationType.HostToken:
                    ValidateHostToken(context, method.Name, argument);
                    break;

                default:
                    break;
            }
        }

        return await next(context);
    }

    private void ValidateGameHash(HubInvocationContext context, string methodName, object argument)
    {
        if (argument is not string gameHash || string.IsNullOrWhiteSpace(gameHash))
        {
            throw new HubException($"{methodName}: Invalid game hash");
        }

        Game game = _gameManager.GetGame(gameHash)
                   ?? throw new HubException($"{methodName}: Game with hash '{gameHash}' does not exist");

        context.Context.Items["Game"] = game;
    }

    private void ValidatePlayerToken(HubInvocationContext context, string methodName, object argument)
    {
        Game game = GetGameFromContext(context, methodName);

        if (argument is not string token || string.IsNullOrWhiteSpace(token))
        {
            throw new HubException($"{methodName}: Invalid player token");
        }

        Player player = _playerManager.GetPlayerByToken(game.GameHash, token)
                     ?? throw new HubException($"{methodName}: Player with token '{token}' does not exist in game '{game.GameHash}'");

        context.Context.Items["Player"] = player;
    }

    private void ValidateDrawingToken(HubInvocationContext context, string methodName, object argument)
    {
        Game game = GetGameFromContext(context, methodName);

        if (argument is not string token || string.IsNullOrWhiteSpace(token) || token != game.GameState.DrawingToken)
        {
            throw new HubException($"{methodName}: Invalid drawing token");
        }

        Player player = _playerManager.GetPlayerByToken(game.GameHash, token)
                     ?? throw new HubException($"{methodName}: Player with token '{token}' does not exist in game '{game.GameHash}'");

        context.Context.Items["Player"] = player;
    }

    private void ValidateHostToken(HubInvocationContext context, string methodName, object argument)
    {
        Game game = GetGameFromContext(context, methodName);

        if (argument is not string token || string.IsNullOrWhiteSpace(token) || token != game.HostToken)
        {
            throw new HubException($"{methodName}: Invalid host token");
        }

        Player player = _playerManager.GetPlayerByToken(game.GameHash, token)
                     ?? throw new HubException($"{methodName}: Player with token '{token}' does not exist in game '{game.GameHash}'");

        context.Context.Items["Player"] = player;
    }

    private static Game GetGameFromContext(HubInvocationContext context, string methodName)
    {
        if (!context.Context.Items.TryGetValue("Game", out var obj) || obj is not Game game)
        {
            throw new HubException($"{methodName}: Game context is missing before player validation");
        }

        return game;
    }
}
