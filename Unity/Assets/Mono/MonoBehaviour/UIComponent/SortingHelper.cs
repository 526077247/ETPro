using UnityEngine;

/// <summary>
/// _sortingOrderSpan 相对偏移量，
/// 1 需要在根特效根节点挂上本脚本并设置好_sortingOrderSpan
/// 2 动态加载特效或者动态加载角色身上的特效
/// 重复1步骤后需要等加载完成后调用下ResetSorting
/// </summary>
public class SortingHelper : MonoBehaviour
{
  
    [SerializeField]

    private int _sortingOrderSpan = 0;

    private int _sortingLayerID = 0;

    private int _sortingOrder = 0;

    public int SortingLayerID
    {
        get => _sortingLayerID;
        set
        {
            _sortingLayerID = value;
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.sortingLayerID = SortingLayerID;
            }
        }
    }

    public int SortingOrder
    {
        get => _sortingOrder;
        set
        {
            _sortingOrder = value;
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.sortingOrder = _sortingOrder;
            }
        }
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null;
        ResetSorting();
    }

    public void ResetSorting()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;
        SortingLayerID = canvas.sortingLayerID;
        SortingOrder = canvas.sortingOrder + _sortingOrderSpan;
    }

    public void SetSortingOrderSpan(int sortingOrderSpan)
    {
        _sortingOrderSpan = sortingOrderSpan;
    }

    public void SetSortingOrderWithOption(int sortingOrderSpan)
    {
        _sortingOrderSpan = sortingOrderSpan;
        ResetSorting();
    }
}
