using AssetKits.ParticleImage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchEffect : Kit.Common.SingletonDontDestroy<TouchEffect>
{
    [Header("References")]
    [SerializeField] ParticleImage m_Particle;
    [SerializeField] RectTransform m_CanvasRect;

    [Header("Click Settings")]
    public float maxClickDuration = 0.3f;
    public float maxClickDistance = 10f;

    private RectTransform rect;

    private float pressTime;
    private Vector2 pressPosition;
    private bool isPressing = false;

    private void Start()
    {
        rect = m_Particle.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rect == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            pressTime = Time.time;
            pressPosition = Input.mousePosition;
            isPressing = true;
        }

        if (Input.GetMouseButtonUp(0) && isPressing)
        {
            float holdTime = Time.time - pressTime;
            float distance = Vector2.Distance(Input.mousePosition, pressPosition);

            if (holdTime <= maxClickDuration && distance <= maxClickDistance)
            {
                ShowParticle();
            }

            isPressing = false;
        }
    }

    private void ShowParticle()
    {
        m_Particle.Play();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, Input.mousePosition, null, out Vector2 vector);
        rect.anchoredPosition = vector;
    }
}
