using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class CanvasSortUI : MonoBehaviour
{
    //[SerializeField]
    public List<Canvas> canvasKeyList = new List<Canvas>();
    //[SerializeField]
    public List<int> canvasValueList = new List<int>();
    //[SerializeField]
    public List<Renderer> rendererKeyList = new List<Renderer>();
    //[SerializeField]
    public List<int> rendererValueList = new List<int>();
    //[SerializeField]
    public List<SortingGroup> sortGroupKeyList = new List<SortingGroup>();
    //[SerializeField]
    public List<int> sortGroupValueList = new List<int>();
    //[SerializeField]
    public List<SpriteRenderer> spriteRendererKeyList = new List<SpriteRenderer>();
    //[SerializeField]
    public List<int> spriteRendererValueList = new List<int>();
    private int rootLayer = 0;

    public void ResetSortInLayer(int rootLayer)
    {
        for(int i=0;i<canvasKeyList.Count;i++)
        {
            if (canvasKeyList[i])
            {

                canvasValueList[i] = canvasValueList[i] + rootLayer - this.rootLayer;
                canvasKeyList[i].sortingOrder = canvasValueList[i];
            }
        }

        for (int i = 0; i < rendererKeyList.Count; i++)
        {
            if (rendererKeyList[i])
            {
                rendererValueList[i] = rendererValueList[i] + rootLayer - this.rootLayer;
                rendererKeyList[i].sortingOrder = rendererValueList[i];
            }
        }
        
        for (int i = 0; i < sortGroupKeyList.Count; i++)
        {
            if (sortGroupKeyList[i])
            {
                sortGroupValueList[i] = sortGroupValueList[i] + rootLayer - this.rootLayer;
                sortGroupKeyList[i].sortingOrder = sortGroupValueList[i];
            }
        }

        for (int i = 0; i < spriteRendererKeyList.Count; i++)
        {
            if (spriteRendererKeyList[i])
            {
                spriteRendererValueList[i] = spriteRendererValueList[i] + rootLayer - this.rootLayer;
                spriteRendererKeyList[i].sortingOrder = spriteRendererValueList[i];
            }
        }

        this.rootLayer = rootLayer;
    }
    

    void Awake()
    {
        InitUICanvas();
        InitUIParticle();
        InitUISortGroup();
        InitUISpriteRenderer();
    }


    void InitUICanvas()
    {
        Canvas[] uiCanvassArr = this.GetComponentsInChildren<Canvas>(true);

        this.canvasKeyList.Clear();
        this.canvasValueList.Clear();
        foreach (Canvas uiCanvas in uiCanvassArr)
        {
            int inStr = uiCanvas.sortingOrder;
            this.canvasKeyList.Add(uiCanvas);
            this.canvasValueList.Add(inStr);
        }
    }

    void InitUIParticle()
    {
        ParticleSystem[] uiCanvassArr = this.GetComponentsInChildren<ParticleSystem>(true);
        this.rendererKeyList.Clear();
        this.rendererValueList.Clear();
        foreach (ParticleSystem uiParticle in uiCanvassArr)
        {
            int inStr = uiParticle.GetComponent<Renderer>().sortingOrder;
            this.rendererKeyList.Add(uiParticle.GetComponent<Renderer>());
            this.rendererValueList.Add(inStr);
        }
    }

    void InitUISortGroup()
    {
        SortingGroup[] sortGroupArr = this.GetComponentsInChildren<SortingGroup>(true);
        this.sortGroupKeyList.Clear();
        this.sortGroupValueList.Clear();
        foreach (SortingGroup item in sortGroupArr)
        {
            int inStr = item.sortingOrder;
            
            this.sortGroupKeyList.Add(item);
            this.sortGroupValueList.Add(inStr);
        }
    }

    void InitUISpriteRenderer()
    {
        SpriteRenderer[] spriteRendererArr = this.GetComponentsInChildren<SpriteRenderer>(true);
        this.spriteRendererKeyList.Clear();
        this.spriteRendererValueList.Clear();
        foreach (SpriteRenderer item in spriteRendererArr)
        {
            int inStr = item.sortingOrder;
            
            this.spriteRendererKeyList.Add(item);
            this.spriteRendererValueList.Add(inStr);
        }
    }

}
