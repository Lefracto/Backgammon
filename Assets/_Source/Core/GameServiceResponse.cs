namespace Core
{
  public enum GameServiceResponse
  {
    IncorrectAttemptToMoveFromHead,
    MoveDoesNotFollowCorrectness,
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