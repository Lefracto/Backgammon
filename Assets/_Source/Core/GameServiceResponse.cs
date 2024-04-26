namespace Core
{
  /// <summary>
  /// Enum for marking service response:
  /// positive values -- correct move
  /// negative values -- incorrect moves
  /// 0 -- no moves
  /// </summary>
  public enum GameServiceResponse
  {
    MustUseTheBiggestDiceValue,
    IncorrectAttemptToLock,
    DestinationIsOccupied,
    AttemptToUseUsedDice,
    NotAllCheckersAtHome,
    IncorrectStartCell,
    NotTheShortestWay,
    ValidCheckerExit,
    ValidCheckerMove,
    UnexpectedError,
    GameFinished,
    NoMoves,
  }
}