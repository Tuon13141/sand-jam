using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerEnterHandler
{
    [SerializeField] float m_SizeWhenScale = 0.9f;
    [SerializeField] float m_DurationScaling = 0.25f;

    Selectable selectableObject;
    Vector3 defaultScale;

    Tween m_ButtonDownAnim;
    Tween m_ButtonUpAnim;

    void Awake()
    {
        selectableObject = GetComponent<Selectable>();
        defaultScale = transform.localScale;
    }

    void OnDisable()
    {
        transform.localScale = defaultScale;
    }

    void OnDestroy()
    {
        if (m_ButtonDownAnim != null) m_ButtonDownAnim.Kill();
        if (m_ButtonUpAnim != null) m_ButtonUpAnim.Kill();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Do nothing if button exists, but is not interactable/enabled.
        if (selectableObject != null && (!selectableObject.interactable || !selectableObject.enabled)) return;
        m_ButtonDownAnim = transform.DOScale(transform.localScale * m_SizeWhenScale, m_DurationScaling).SetUpdate(true).OnComplete(() => m_ButtonDownAnim = null);
    }

    public async void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("OnPointerUp");
        if (m_ButtonDownAnim != null) await m_ButtonDownAnim.AsyncWaitForCompletion();
        if (this != null) m_ButtonUpAnim = transform.DOScale(defaultScale, m_DurationScaling).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("OnPointerExit");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("OnPointerEnter");
    }
}
