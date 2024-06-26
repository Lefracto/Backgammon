using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core;
using Presentation;

public class GameFieldView : MonoBehaviour, IPossibleMovesIndicator
{
  [SerializeField] private List<Transform> _segments;
  [SerializeField] private Transform[] _checkersOut;

  [Space(15)] [SerializeField] private GameObject _whiteChecker;
  [SerializeField] private GameObject _blackChecker;
  [SerializeField] private GameObject _fieldIndicator;
  [SerializeField] private Vector3 _checkerHighlightOffset;

  [Space(15)] [SerializeField] private List<CheckerView> _checkerViews;

  private List<GameObject> _highLightedCheckers;
  private Action<int> _onSelectCheckerHandler;
  private Action<int[]> _onAvailableSegmentClickHandler;


  [Inject]
  public void Init(IGameDataProvider service, Action<int> onSelectCheckerHandler,
    Action<int[]> onAvailableSegmentClickHandler)
  {
    service.OnNewGameDataReceived += RedrawField;
    _onAvailableSegmentClickHandler = onAvailableSegmentClickHandler;
    _onSelectCheckerHandler = onSelectCheckerHandler;
    _highLightedCheckers = new List<GameObject>();
  }

  private void ClearField()
  {
    foreach (Transform t in _segments)
      while (t.childCount != 0)
        DestroyImmediate(t.GetChild(0).gameObject);

    foreach (Transform t in _checkersOut)
      while (t.childCount != 0)
        DestroyImmediate(t.GetChild(0).gameObject);
  }


  private void InitField(GameData data)
  {
    _checkerViews = new List<CheckerView>(24);
    for (int index = 0; index < data.Checkers.Length; index++)
    {
      Checker t = data.Checkers[index];
      GameObject checker = t.PlayerId == 0 ? _whiteChecker : _blackChecker;
      Transform parent = t.Position == 24 ? _checkersOut[t.PlayerId] : _segments[t.Position];
      GameObject checkerInstance = Instantiate(checker, Vector3.zero, Quaternion.identity, parent);

      if (checkerInstance.TryGetComponent(out CheckerView checkerView) is false)
        Debug.LogError("CheckerView component is not found on the checker instance");

      _checkerViews.Add(checkerView);
      _checkerViews[index].Init(t, _onSelectCheckerHandler);
    }
  }

  private bool _firstStart = true;

  public void Restart()
  {
    _firstStart = true;
    ClearField();
  }

  private bool _isAnimationInProcess;

  private void RedrawField(GameData data)
  {
    ClearHighlights();

    if (_firstStart)
    {
      InitField(data);
      _firstStart = false;
      return;
    }

    if (data.LastChangedCheckerId == -1) return;
    CheckerView changedView = _checkerViews.FirstOrDefault(view => view.GetCheckerId() == data.LastChangedCheckerId);
    int index = data.Checkers.FirstOrDefault(c => c.Id == data.LastChangedCheckerId)!.Position;
    
    int k = index < 12 ? -1 : 1;
    
    if (_isAnimationInProcess is false)
    {
      changedView!.TransferChecker(_segments[index], _segments[index].childCount * k);
      _isAnimationInProcess = true;
    }
    else
    {
      _isAnimationInProcess = false;
      
    }
  }

  public void HighlightAvailableCheckers(List<PossibleMove> possibleMoves)
  {
    foreach (PossibleMove possibleMove in possibleMoves)
    {
      GameObject highlight = Instantiate(_fieldIndicator, _segments[possibleMove.Destination].transform.position,
        Quaternion.identity,
        _segments[possibleMove.Destination].transform.parent);
      const int upperFieldsIndex = 12;

      if (highlight.TryGetComponent(out HighlightButton button))
      {
        button.Init(_onAvailableSegmentClickHandler, possibleMove.DiceToUse.ToArray());
      }
      else
        Debug.LogError("Highlight prefab does not contain Button component!");

      if (possibleMove.Destination < upperFieldsIndex)
      {
        highlight.transform.Rotate(new Vector3(0, 0, -180));
        highlight.transform.localPosition += _checkerHighlightOffset;
      }
      else
        highlight.transform.localPosition -= _checkerHighlightOffset;


      _highLightedCheckers.Add(highlight);
    }
  }

  private void ClearHighlights()
  {
    foreach (GameObject highlight in _highLightedCheckers)
      Destroy(highlight);
    _highLightedCheckers.Clear();
  }
}