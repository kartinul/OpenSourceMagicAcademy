using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class FlexLayoutGroup : MonoBehaviour
{
    [Header("Layout Settings")]
    [SerializeField] private int maxColumnsBeforeWrap = 3;
    [SerializeField] private float minItemHeight = 50f;
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);
    [SerializeField] private RectOffset padding;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (padding == null)
        {
            padding = new RectOffset(10, 10, 10, 10);
        }
    }

    private void OnEnable()
    {
        CalculateLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        CalculateLayout();
    }

    private void OnTransformChildrenChanged()
    {
        ForceLayoutRefresh();
    }

    public void ForceLayoutRefresh()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(DeferredRefreshRoutine());
    }

    private IEnumerator DeferredRefreshRoutine()
    {
        yield return new WaitForEndOfFrame();
        CalculateLayout();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void CalculateLayout()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        int activeCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                activeCount++;
        }

        if (activeCount == 0) return;

        int columns = activeCount > maxColumnsBeforeWrap ? 2 : 1;
        int rows = Mathf.CeilToInt((float)activeCount / columns);

        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;

        int padLeft = padding != null ? padding.left : 0;
        int padRight = padding != null ? padding.right : 0;
        int padTop = padding != null ? padding.top : 0;
        int padBottom = padding != null ? padding.bottom : 0;

        float usableWidth = panelWidth - padLeft - padRight - (spacing.x * (columns - 1));
        float usableHeight = panelHeight - padTop - padBottom - (spacing.y * (rows - 1));

        float itemWidth = usableWidth / columns;
        float itemHeight = Mathf.Max(minItemHeight, usableHeight / rows);

        int currentActiveIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            int row = currentActiveIndex / columns;
            int col = currentActiveIndex % columns;

            child.anchorMin = new Vector2(0, 1);
            child.anchorMax = new Vector2(0, 1);
            child.pivot = new Vector2(0, 1);

            float posX = padLeft + col * (itemWidth + spacing.x);
            float posY = -padTop - row * (itemHeight + spacing.y);

            child.anchoredPosition = new Vector2(posX, posY);
            child.sizeDelta = new Vector2(itemWidth, itemHeight);

            currentActiveIndex++;
        }
    }
}