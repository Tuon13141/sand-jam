using UnityEngine;

public class SpriteElement : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_SpriteRenderer;
    public SpriteRenderer spriteRenderer => m_SpriteRenderer;

    public Vector2 gridPosition;
}
