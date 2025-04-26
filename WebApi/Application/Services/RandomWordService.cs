using WebApi.Api.Models.HttpResponse;
using WebApi.Api.Utilities;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services.Interfaces;
using WebApi.Domain.Entities;
using WebApi.Domain.Static;
using WebApi.Infrastructure.Repositories.Interfaces;

namespace WebApi.Application.Services;

public class RandomWordService : IRandomWordService
{
    private readonly IWordRepository _wordRepository;
    private readonly IGameManager _gameManager;

    public RandomWordService(
        IWordRepository wordRepository,
        IGameManager gameManager)
    {
        _wordRepository = wordRepository;
        _gameManager = gameManager;
    }

    public async Task<string> FetchWordAsync(string gameHash, CancellationToken cancellationToken)
    {
        Game game = _gameManager.GetGame(gameHash);

        switch (game.GameSettings.WordLanguage)
        {
            case Languages.EN:
                return await _wordRepository.GetRandomWordAsync(Languages.EN, cancellationToken);
            case Languages.PL:
                return await _wordRepository.GetRandomWordAsync(Languages.PL, cancellationToken);
            default:
                return await _wordRepository.GetRandomWordAsync(Languages.EN, cancellationToken);
        }
    }
}