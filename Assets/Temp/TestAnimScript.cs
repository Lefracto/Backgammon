using DG.Tweening;
using UnityEngine;

public class TestAnimScript : MonoBehaviour
{
  [SerializeField] Transform _targetObject;

  private void Start()
  {
    MoveToTarget();
  }

  private void MoveToTarget()
  {
    transform.DOMove(_targetObject.position, 5f);
  }
}