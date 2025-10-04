using Microsoft.AspNetCore.SignalR;
using WebApi.Domain.Entities;

namespace WebApi.Api.Hubs.Extensions
{
    public static class HubContextExtensions
    {
        private const string GameKey = "Game";
        private const string PlayerKey = "Player";

        public static Game? GetGame(this HubCallerContext context)
        {
            if (context.Items.TryGetValue(GameKey, out var value) && value is Game game)
            {
                return game;
            }

            return null;
        }

        public static void SetGame(this HubCallerContext context, Game game)
        {
            context.Items[GameKey] = game;
        }

        public static Player? GetPlayer(this HubCallerContext context)
        {
            if (context.Items.TryGetValue(PlayerKey, out var value) && value is Player player)
            {
                return player;
            }

            return null;
        }

        public static void SetPlayer(this HubCallerContext context, Player player)
        {
            context.Items[PlayerKey] = player;
        }
    }
}
