namespace Spacer.Application.Ports;

using Spacer.Application.GameState;

public interface IGameTimeStore
{
    GameTimeSnapshot? Load();
    void Save(GameTimeSnapshot snapshot);
}
