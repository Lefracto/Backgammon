using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameFieldView : MonoBehaviour
{
  [SerializeField] private List<Transform> _segments;
  [SerializeField] private GameObject _whiteChecker;
  [SerializeField] private GameObject _blackChecker;
  
  [Inject]
  public void Init(IGameDataProvider service)
    => service.OnNewGameDataReceived += RedrawField;
  
  // TODO: It would be a good idea to update the field only the part that has been updated,
  // TODO: because this computing too hard

  private void ClearField()
  {
    for (int i = 0; i < _segments.Count; i++)
      while (_segments[i].childCount != 0)
        DestroyImmediate(_segments[i].GetChild(0).gameObject);
  }

  private void RedrawField(GameData data)
  {
    ClearField();
    for (int i = 0; i < _segments.Count; i++)
    {
      for (int j = 0; j < data.Field[i].CountCheckers; j++)
      {
        GameObject checker = data.Field[i].PlayerId == 0 ? _whiteChecker : _blackChecker;
        Instantiate(checker, Vector3.zero, Quaternion.identity, _segments[i].transform);
      }
    }
  }
}