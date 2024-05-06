using System;
using Core;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
  [RequireComponent(typeof(Button))]
  public class CheckerView : MonoBehaviour
  {
    [SerializeField] private Vector3 _queueDelta;
    [SerializeField] private GameObject _empty;
    
    private Checker _checker;
    [SerializeField] private Button _button;
    private event Action<int> OnSelectChecker;

    [Header("For available to use checker")] [SerializeField]
    private SpriteState _spriteStateForAvailable;

    [Space(20)] [Header("For unavailable to use checker")] [SerializeField]
    private SpriteState _spriteStateForUnavailable;

    private void UpdateSpriteState(bool isAvailable)
      => _button.spriteState = isAvailable ? _spriteStateForAvailable : _spriteStateForUnavailable;

    public void OnSelect()
    {
      Debug.Log($"OnSelect, position -- {_checker.Position}");
      if (_checker.IsTheUpperOnPosition.Value)
        OnSelectChecker?.Invoke(_checker.Id);
    }

    public void Init(Checker checker, Action<int> onSelectChecker)
    {
      _checker = checker;
      _checker.IsTheUpperOnPosition.Subscribe(UpdateSpriteState);
      OnSelectChecker += onSelectChecker;
    }

    public void TransferChecker(Transform newParent, int queueNumber)
    {
      GameObject emptyObject = Instantiate(_empty, newParent);

      transform.SetParent(transform.parent.parent);
      //emptyObject.transform.SetParent(emptyObject.transform.parent.parent);
      Debug.Log("Transfer");
      Debug.Log($"Animated position - {_empty.transform.localPosition}");

      var position = emptyObject.transform.position += queueNumber * _queueDelta;
      transform.DOMove(position, 2.0f).OnComplete(() =>
      {
        transform.SetParent(newParent);
        Destroy(emptyObject);
      });
    }

    public int GetCheckerId()
      => _checker.Id;
  }
}