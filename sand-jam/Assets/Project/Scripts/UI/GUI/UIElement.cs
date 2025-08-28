using System;
using UnityEngine;

[DefaultExecutionOrder(10)]
public abstract class UIElement : MonoBehaviour
{
    private Action onHidden;
    public abstract bool ManualHide { get; }
    public abstract bool DestroyOnHide { get; }
    public abstract bool UseBehindPanel { get; }
    [SerializeField] protected GameObject holder;
    public virtual void Show(Action hidden)
    {
        onHidden = hidden;
        Show();
    }
    public virtual void Show()
    {
        GameUI.instance.Submit(this);
        holder?.SetActive(true);

    }
    public virtual void Hide()
    {
        GameUI.instance.Unsubmit(this);
        onHidden?.Invoke();
        if (DestroyOnHide)
        {
            GameUI.instance.Unregister(this);
            Destroy(gameObject);
        }
        else holder?.SetActive(false);
    }
    protected virtual void Awake()
    {
        GameUI.instance.Register(this);
    }
}