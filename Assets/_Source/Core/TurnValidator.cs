using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;

namespace Core
{
  // ReSharper disable once ClassNeverInstantiated.Global
  public class TurnValidator : ITurnValidator
  {
    private static readonly (int, int) white_house = (0, 5);
    private static readonly (int, int) black_house = (12, 17);

    private const int OUT_OF_BOARD = 24;
    private const int WHITE_HEAD = 23;
    private const int BLACK_HEAD = 11;

    private GameData _actualData;
    private GameServiceResponse ActualResponse { get; set; }
    private bool _firstHeadMoveStatus;
    private TestMoveLog _lastTestMove;


    /// <summary>
    /// The method validates the move according to long backgammon rules.
    /// </summary>
    /// <param name="cell"> The cell from which try to move the checker. </param>
    /// <param name="diceId"> ID of the cube used for the move. </param>
    /// <param name="data"> Actual GameData with field positions info. </param>
    /// <returns> true -- turn is valid, otherwise -- false. </returns>
    public int ValidateTurn(int cell, int diceId, GameData data)
    {
      _actualData = data;
      _firstHeadMoveStatus = data.MayMoveFromHead;

      int possibleDestination = -1;

      Func<bool>[] validations =
      {
        () => IsValidStartCell(cell),
        () => IsValidCube(diceId),
        () => IsOkayHeadMove(cell),
        () => IsDestinationOkay(possibleDestination = GetDestinationCell(cell, data.DicesResult[diceId])),
        () => IsOkayLocking(cell, possibleDestination),
        () => IsMoveFollowingCompleteness(_actualData.Checkers.First(checker => checker.Position == cell),
          possibleDestination, diceId)
      };

      bool isTurnValid = validations.All(validation => validation());
      data.Response = ActualResponse;
      return isTurnValid ? possibleDestination : -1;
    }

    /// <summary>
    /// The method calculates the index on the segment of the field where you want to move
    /// the checker based on the value of the cube and checker output rules. Only correct parameters!!! 
    /// </summary>
    /// <param name="cell"> The cell from which move the checker. </param>
    /// <param name="diceValue"> Value on dice. </param>
    /// <returns> Index of destination segment. </returns>
    private int GetDestinationCell(int cell, int diceValue)
    {
      int destination = GetDestinationSegmentIndex(cell, diceValue);

      if (destination < 0)
      {
        if (IsAllCheckersInHome() is false)
          return -1;

        return destination == -1 || IsTheNearestToOutChecker(destination) ? OUT_OF_BOARD : -1;
      }

      Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == destination);
      if (checker == null || checker.PlayerId == _actualData.PlayerIdInTurn)
        return destination;

      ActualResponse = GameServiceResponse.DestinationIsOccupied;
      return -1;
    }

    private bool IsOkayHeadMove(int startCell)
    {
      if (startCell != WHITE_HEAD && startCell != BLACK_HEAD)
        return true;

      if (_actualData.CountMoves is 0 or 1)
        return CheckIfSingleCheckerCanMove();

      if (!_actualData.MayMoveFromHead)
      {
        ActualResponse = GameServiceResponse.IncorrectAttemptToMoveFromHead;
        return false;
      }

      _actualData.MayMoveFromHead = false;
      return true;
    }

    private bool CheckIfSingleCheckerCanMove(bool writeToData = true)
    {
      var checkers = _actualData.Checkers
        .Where(checker => checker.PlayerId == _actualData.PlayerIdInTurn &&
                          checker.Position != WHITE_HEAD &&
                          checker.Position != BLACK_HEAD)
        .ToArray();

      switch (checkers.Length)
      {
        case 0:
          if (writeToData)
            _actualData.MayMoveFromHead = false;
          return true;
        case > 1:
          ActualResponse = GameServiceResponse.IncorrectAttemptToMoveFromHead;
          return false;
        default:
          return CanFullyMoveFromCurrentPosition(checkers[0]);
      }
    }

    private bool CanFullyMoveFromCurrentPosition(Checker checker)
    {
      int start = checker.Position;
      int[] nonZeroDicesValues = _actualData.DicesResult.Where(dice => dice != 0).ToArray();

      foreach (int diceValue in nonZeroDicesValues)
      {
        int destination = GetDestinationSegmentIndex(start, diceValue);
        Checker checkerOnDestination = _actualData.Checkers.FirstOrDefault(c => c.Position == destination);

        if (checkerOnDestination != null && checkerOnDestination.PlayerId != _actualData.PlayerIdInTurn)
          return true;

        start = destination;
      }

      return false;
    }

    // TODO: fully test
    private bool IsOkayLocking(int cell, int destination)
    {
      int streakStart = FindLock(cell, destination);
      if (streakStart == -1)
        return true;

      for (int i = streakStart; i < 23; i++)
      {
        Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == i);
        if (checker != null && checker.PlayerId != _actualData.PlayerIdInTurn)
          return true;
      }

      ActualResponse = GameServiceResponse.IncorrectAttemptToLock;
      return false;
    }

    // TODO: fully test
    /// <summary>
    /// Method for searching the streak of 6 checkers in the field, if you make the next move. It gets only correct data!
    /// </summary>
    /// <param name="cell">Start cell, for making possible move</param>
    /// <param name="destination">Destination for possible move</param>
    /// <returns>Index of streak's start or -1 if it does not exist.</returns>
    private int FindLock(int cell, int destination)
    {
      const int streakLength = 6;
      const int lastFieldIndex = 23;

      Checker startChecker = _actualData.Checkers.First(checker => checker.Position == cell);
      startChecker!.Position = destination;

      int i = destination - streakLength;
      for (i = Math.Max(0, i); i <= destination; i++)
      {
        int streak = 0;
        for (int j = i; j < i + streakLength; j++)
        {
          if (i > lastFieldIndex) break;
          Checker currentChecker = _actualData.Checkers.FirstOrDefault(checker1 => checker1.Position == j);
          if (currentChecker is null || currentChecker.PlayerId != _actualData.PlayerIdInTurn)
          {
            i = j;
            break;
          }

          streak++;
        }

        if (streak != streakLength) continue;
        startChecker!.Position = cell;
        return i + streakLength;
      }

      startChecker!.Position = cell;
      return -1;
    }

    private bool IsTheNearestToOutChecker(int currentDistance)
    {
      (int, int) house = GetHouse();
      var distances = new List<int>();
      for (int i = house.Item1; i <= house.Item2; i++)
      {
        int j;
        for (j = 0; j < _actualData.DicesResult.Length; j++)
        {
          int t = _actualData.DicesResult[j];
          Checker possibleChecker = _actualData.Checkers.FirstOrDefault(checker =>
            checker.Position == i && checker.PlayerId == _actualData.PlayerIdInTurn);
          if (t != 0 && possibleChecker is not null)
            distances.Add(GetDestinationSegmentIndex(i, t));
        }
      }

      // TODO:  
      bool isThisNearestChecker =
        distances.Any(distance => distance > currentDistance) is false;

      if (isThisNearestChecker is false)
        ActualResponse = GameServiceResponse.NotTheShortestWay;

      return isThisNearestChecker;
    }

    private bool IsValidStartCell(int cell)
    {
      const int lastFieldIndex = 23;
      if (cell is < 0 or > lastFieldIndex)
      {
        ActualResponse = GameServiceResponse.UnexpectedError;
        return false;
      }

      Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == cell);
      if (checker is not null && checker.PlayerId == _actualData.PlayerIdInTurn)
        return true;

      ActualResponse = GameServiceResponse.IncorrectStartCell;
      return false;
    }

    private bool IsValidCube(int cubeId)
    {
      if (cubeId < 0 || cubeId > _actualData.DicesResult.Length)
      {
        ActualResponse = GameServiceResponse.UnexpectedError;
        return false;
      }

      bool isThisDiceUsed = _actualData.DicesResult[cubeId] == 0;

      if (isThisDiceUsed)
        ActualResponse = GameServiceResponse.AttemptToUseUsedDice;

      return !isThisDiceUsed;
    }

    private bool IsAllCheckersInHome()
    {
      const int countCheckers = 30;
      (int, int) house = GetHouse();
      for (int i = 0; i < countCheckers; i++)
      {
        if (_actualData.Checkers[i].PlayerId != _actualData.PlayerIdInTurn ||
            (_actualData.Checkers[i].Position >= house.Item1 &&
             _actualData.Checkers[i].Position <= house.Item2) ||
            _actualData.Checkers[i].Position == OUT_OF_BOARD)
          continue;

        ActualResponse = GameServiceResponse.NotAllCheckersAtHome;
        return false;
      }

      return true;
    }

    /// <summary>
    /// Calculates the index of the segment where the checker will be moved.
    /// </summary>
    /// <param name="startCell">Current checker cell</param>
    /// <param name="value">Value of dice, which is going to be used</param>
    /// <returns>Index of field's segment</returns>
    private int GetDestinationSegmentIndex(int startCell, int value)
    {
      const int halfOfField = 12;
      (int, int) convertedIndex;
      convertedIndex.Item1 = startCell / halfOfField;
      convertedIndex.Item2 = startCell - convertedIndex.Item1 * halfOfField;

      convertedIndex.Item2 -= value;
      switch (convertedIndex.Item2)
      {
        case < 0 when convertedIndex.Item1 != _actualData.PlayerIdInTurn:
          convertedIndex.Item2 += halfOfField;
          convertedIndex.Item1 = _actualData.PlayerIdInTurn;
          break;
        case < 0 when convertedIndex.Item1 == _actualData.PlayerIdInTurn:
          return convertedIndex.Item2;
      }

      return convertedIndex.Item1 * halfOfField + convertedIndex.Item2;
    }

    public bool IsTherePossibleMoves()
    {
      bool mayMoveFromHead = _actualData.MayMoveFromHead;

      int[] uniquePositions = _actualData.Checkers
        .Where(checker => checker.PlayerId == _actualData.PlayerIdInTurn)
        .Select(checker => checker.Position)
        .Distinct()
        .ToArray();

      int[] nonZeroDiceIndexes = _actualData.DicesResult
        .Where(dice => dice != 0)
        .Distinct()
        .Select(i => _actualData.DicesResult.IndexOf(i))
        .ToArray();

      // TODO: Change double foreach for IsPossibleToUseDice
      foreach (int position in uniquePositions)
      {
        foreach (int diceId in nonZeroDiceIndexes)
        {
          // Fast check move for correctness. It does it for each position &  
          int destination = GetDestinationCell(position, _actualData.DicesResult[diceId]);
          if (!IsOkayHeadMove(position) || !IsOkayLocking(position, destination) || destination == -1) continue;
          _actualData.MayMoveFromHead = mayMoveFromHead;
          return true;
        }
      }

      _actualData.MayMoveFromHead = mayMoveFromHead;
      ActualResponse = GameServiceResponse.NoMoves;
      return false;
    }

    /// <summary>
    /// Method checks the completeness rule for move.
    /// Completeness rule: player must use the biggest possible count of points. 
    /// </summary>
    /// <returns>true -- if maintain the rule, otherwise -- false</returns>
    private bool IsMoveFollowingCompleteness(Checker startChecker, int destination, int diceId)
    {
      if (CanMoveBeIncomplete(destination) is false)
        return true;

      MakeTestMove(startChecker, destination, _actualData.DicesResult[diceId]);

      if (IsRemainingMovePossible())
      {
        CancelLastTestMove();
        return true;
      }

      if (IsMoveBlockingFullTurn())
      {
        CancelLastTestMove();
        ActualResponse = GameServiceResponse.MoveDoesNotFollowCorrectness;
        return false;
      }

      if (IsUsingHighestValueDice(diceId))
        return true;

      CancelLastTestMove();
      ActualResponse = GameServiceResponse.MoveDoesNotFollowCorrectness;
      return false;
    }

    private bool CanMoveBeIncomplete(int destination)
    {
      return !(_actualData.DicesResult.Count(dice => dice != 0) == 1 ||
               _actualData.DicesResult.Where(dice => dice != 0).Distinct().Count() == 1 ||
               destination == OUT_OF_BOARD);
    }

    private bool IsRemainingMovePossible()
    {
      int lastNonZeroDiceValue = _actualData.DicesResult.First(dice => dice != 0);
      return IsPossibleToUseDice(lastNonZeroDiceValue, false);
    }

    private bool IsMoveBlockingFullTurn()
    {
      _actualData.MayMoveFromHead = _firstHeadMoveStatus;
      bool fullMovePossible = IsFullMovePossible();
      _actualData.MayMoveFromHead = _firstHeadMoveStatus;
      return fullMovePossible;
    }

    private bool IsUsingHighestValueDice(int diceId)
    {
      int maxDiceValue = _actualData.DicesResult.Max();
      int necessaryDiceIndex = _actualData.DicesResult.IndexOf(_actualData.DicesResult.Min());
      if (IsPossibleToUseDice(maxDiceValue, false))
        necessaryDiceIndex = _actualData.DicesResult.IndexOf(maxDiceValue);
      return necessaryDiceIndex == diceId;
    }

    private bool IsFullMovePossible()
    {
      // If there is only one cube left, then the completeness of the stroke is limited to the ability to resemble this cube
      if (_actualData.DicesResult.Count(dice => dice != 0) == 1)
        return true;

      var uniqueDiceValues = _actualData.DicesResult.Where(dice => dice != 0).Distinct();
      foreach (int dieValue in uniqueDiceValues)
      {
        if (!IsPossibleToUseDice(dieValue, true) || !IsTherePossibleMoves()) continue;
        CancelLastTestMove();
        return true;
      }

      return false;
    }

    private bool IsPossibleToUseDice(int diceValue, bool makeTurn)
    {
      bool mayMoveFromHead = _actualData.MayMoveFromHead;
      int[] uniquePositions = _actualData.GetUniquePositions();

      foreach (int position in uniquePositions)
      {
        int destination = GetDestinationCell(position, diceValue);
        if (!IsOkayHeadMove(position) || !IsOkayLocking(position, destination) || destination == -1)
        {
          _actualData.MayMoveFromHead = mayMoveFromHead;
          continue;
        }

        if (makeTurn)
          MakeTestMove(_actualData.Checkers.First(checker => checker.Position == position), destination, diceValue);
        return true;
      }

      return false;
    }

    private void MakeTestMove(Checker checker, int destination, int diceValue)
    {
      _lastTestMove = new TestMoveLog(checker.Id, checker.Position, _actualData.MayMoveFromHead, diceValue);
      checker.Position = destination;
      _actualData.DicesResult[_actualData.DicesResult.IndexOf(diceValue)] = 0;
    }

    private void CancelLastTestMove()
    {
      _actualData.Checkers.First(checker => checker.Id == _lastTestMove.ChangedCheckerId).Position =
        _lastTestMove.PreviousPosition;
      _actualData.MayMoveFromHead = _lastTestMove.HeadMoveStatus;
      _actualData.DicesResult[_actualData.DicesResult.IndexOf(0)] = _lastTestMove.DiceValue;
    }

    private bool IsDestinationOkay(int destination)
      => destination != -1;

    public List<PossibleMove> GetPossibleMoves(int cell, GameData data)
    {
      var moves = new List<PossibleMove>();
      for (int i = 0; i < data.DicesResult.Length; i++)
      {
        int destination = ValidateTurn(cell, i, data);
        if (destination != -1)
        {
          // Multiple dice check
          moves.Add(new PossibleMove(destination, new[] { i }));
        }
      }

      return moves;
    }

    private (int, int) GetHouse()
      => _actualData.PlayerIdInTurn == 0 ? white_house : black_house;
  }
}