namespace Core
{
  public enum GameServiceResponse
  {
    IncorrectAttemptToMoveFromHead,
    MustUseTheBiggestDiceValue,
    MustUseAllDices,
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