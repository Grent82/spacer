namespace Spacer.Application.UseCases;

using Spacer.Application.GameState;
using Spacer.Application.Ports;

public sealed class SaveGameUseCase
{
    private readonly IGameTime _gameTime;
    private readonly IGameTimeStore _gameTimeStore;

    public SaveGameUseCase(IGameTime gameTime, IGameTimeStore gameTimeStore)
    {
        _gameTime = gameTime;
        _gameTimeStore = gameTimeStore;
    }

    public void Execute()
    {
        var snapshot = new GameTimeSnapshot(_gameTime.Year, _gameTime.Month, _gameTime.MonthsInYear);
        _gameTimeStore.Save(snapshot);
    }
}
