using System;

public interface IGameDataProvider
{
  event Action<GameData> OnNewGameDataReceived;
}