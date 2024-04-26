using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Core;

public class GameFieldView : MonoBehaviour
{
  [SerializeField] private List<Transform> _segments;
  [SerializeField] private Transform[] _checkersOut;
  [Space(15)]
  [SerializeField] private GameObject _whiteChecker;
  [SerializeField] private GameObject _blackChecker;
  
  [Inject]
  public void Init(IGameDataProvider service)
    => service.OnNewGameDataReceived += RedrawField;
  
  private void ClearField()
  {
    foreach (Transform t in _segments)
      while (t.childCount != 0)
        DestroyImmediate(t.GetChild(0).gameObject);
    
    foreach (Transform t in _checkersOut)
      while (t.childCount != 0)
        DestroyImmediate(t.GetChild(0).gameObject);
  }

  private void RedrawField(GameData data)
  {
    ClearField();
    foreach (Checker t in data.Checkers)
    {
      GameObject checker = t.PlayerId == 0 ? _whiteChecker : _blackChecker;
      Transform parent = t.Position == 24 ? _checkersOut[t.PlayerId] : _segments[t.Position];
      Instantiate(checker, Vector3.zero, Quaternion.identity, parent);
    }
  }
}