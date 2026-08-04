using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform knob;
    private Vector2 inputVector;

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        float radius = GetComponent<RectTransform>().sizeDelta.x * 0.5f;
        inputVector = pos / radius;
        if (inputVector.magnitude > 1f)
            inputVector = inputVector.normalized;

        knob.anchoredPosition = inputVector * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
    }

    public Vector2 GetInput()
    {
        return inputVector;
    }
}