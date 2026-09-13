using UnityEngine;
using UnityEngine.UI;

/// <summary>Fits the prefab's two-column lesson grid to its actual viewport width.</summary>
[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup))]
public sealed class LessonGridLayout : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup grid;
    private void OnEnable() => Resize();
    private void OnRectTransformDimensionsChange() => Resize();
    private void Resize()
    {
        if (grid == null) return;
        float width = ((RectTransform)transform).rect.width;
        float cellWidth = Mathf.Max(1, (width - grid.padding.horizontal - grid.spacing.x) / 2f);
        if (!Mathf.Approximately(grid.cellSize.x, cellWidth))
            grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
    }
#if UNITY_EDITOR
    public void Configure(GridLayoutGroup layout) { grid = layout; Resize(); }
#endif
}
