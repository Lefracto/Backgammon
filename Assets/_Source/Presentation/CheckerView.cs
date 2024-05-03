using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
  [RequireComponent(typeof(Button))]
  public class CheckerView : MonoBehaviour
  {
    // TODO: Use UniRx instead of events
    private Checker _checker;
    private bool _isHighlighted;
    private Button _button;

    [Header("For available to use checker")] [SerializeField]
    private SpriteState _spriteStateForAvailable;

    [Space(20)] [Header("For unavailable to use checker")] [SerializeField]
    private SpriteState _spriteStateForUnavailable;

    private bool _mayBeUsedForMove;

    public bool MayBeUsedForMove
    {
      get => _mayBeUsedForMove;
      set
      {
        _button.spriteState = value ? _spriteStateForAvailable : _spriteStateForUnavailable;
        _mayBeUsedForMove = value;
      }
    }

    private void Awake()
    {
      _button = GetComponent<Button>();
      MayBeUsedForMove = false;
    }

    public void Init(Checker checker)
      => _checker = checker;

    // TODO: For animation
    public void TransferChecker()
    {
    }
  }
}