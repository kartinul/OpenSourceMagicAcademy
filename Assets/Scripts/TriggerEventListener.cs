using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TriggerEventListener : MonoBehaviour
{
    public enum FilterType { None, Tag, Layer }

    [Header("Filter Settings")]
    [SerializeField] private FilterType filterType = FilterType.None;
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private LayerMask targetLayer;

    [Header("Trigger Events")]
    public UnityEvent<Collider2D> onTriggerEnter;
    public UnityEvent<Collider2D> onTriggerExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValid(other))
        {
            onTriggerEnter?.Invoke(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValid(other))
        {
            onTriggerExit?.Invoke(other);
        }
    }

    private bool IsValid(Collider2D other)
    {
        switch (filterType)
        {
            case FilterType.Tag:
                return other.CompareTag(targetTag);

            case FilterType.Layer:
                return (targetLayer.value & (1 << other.gameObject.layer)) != 0;

            default:
                return true;
        }
    }
}