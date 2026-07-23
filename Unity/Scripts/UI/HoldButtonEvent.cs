using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldButtonEvent : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    public UnityEvent onHoldStart;
    public UnityEvent onHoldEnd;

    public void OnPointerDown(PointerEventData eventData)
    {
        onHoldStart?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onHoldEnd?.Invoke();
    }
}