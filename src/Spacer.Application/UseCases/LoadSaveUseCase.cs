namespace Spacer.Application.UseCases;

using Spacer.Application.Ports;

public sealed class LoadSaveUseCase
{
    private readonly IGameTime _gameTime;
    private readonly IGameTimeStore _gameTimeStore;

    public LoadSaveUseCase(IGameTime gameTime, IGameTimeStore gameTimeStore)
    {
        _gameTime = gameTime;
        _gameTimeStore = gameTimeStore;
    }

    public bool Execute()
    {
        var snapshot = _gameTimeStore.Load();
        if (snapshot is null)
        {
            return false;
        }

        _gameTime.Set(snapshot.Year, snapshot.Month);
        return true;
    }
}
