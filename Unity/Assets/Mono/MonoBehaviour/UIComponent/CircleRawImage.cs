using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
 
 
public class CircleRawImage : RawImage
{
    public float Radius = 2f;//内切圆半径 图片的一半差不多就是一个圆了 这里相当于图片十分之一的长度
    public int TriangleNum = 36;//每个扇形三角形个数 个数越大弧度越平滑
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        float tw = rectTransform.rect.width;//图片的宽
        float th = rectTransform.rect.height;//图片的高
        float twr = tw / 2;
        float thr = th / 2;

        if (Radius < 0)
            Radius = 0;
        float radius = tw / Radius;//半径这里需要动态计算确保不会被拉伸
        if (radius > twr)
            radius = twr;
        if (radius < 0)
            radius = 0;
        if (TriangleNum <= 0)
            TriangleNum = 1;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        //左边矩形
        AddVert(new Vector2(-twr, -thr + radius), tw, th, vh, vert);
        AddVert(new Vector2(-twr, thr - radius), tw, th, vh, vert);
        AddVert(new Vector2(-twr + radius, thr - radius), tw, th, vh, vert);
        AddVert(new Vector2(-twr + radius, -thr + radius), tw, th, vh, vert);
        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
        //中间矩形
        AddVert(new Vector2(-twr + radius, -thr), tw, th, vh, vert);
        AddVert(new Vector2(-twr + radius, thr), tw, th, vh, vert);
        AddVert(new Vector2(twr - radius, thr), tw, th, vh, vert);
        AddVert(new Vector2(twr - radius, -thr), tw, th, vh, vert);
        vh.AddTriangle(4, 5, 6);
        vh.AddTriangle(4, 6, 7);
        //右边矩形
        AddVert(new Vector2(twr - radius, -thr + radius), tw, th, vh, vert);
        AddVert(new Vector2(twr - radius, thr - radius), tw, th, vh, vert);
        AddVert(new Vector2(twr, thr - radius), tw, th, vh, vert);
        AddVert(new Vector2(twr, -thr + radius), tw, th, vh, vert);
        vh.AddTriangle(8, 9, 10);
        vh.AddTriangle(8, 10, 11);

        List<Vector2> CirclePoint = new List<Vector2>();//圆心列表
        Vector2 pos0 = new Vector2(-twr + radius, -thr + radius);//左下角圆心
        Vector2 pos1 = new Vector2(-twr, -thr + radius);//决定首次旋转方向的点
        Vector2 pos2;
        CirclePoint.Add(pos0);
        CirclePoint.Add(pos1);
        pos0 = new Vector2(-twr + radius, thr - radius);//左上角圆心
        pos1 = new Vector2(-twr + radius, thr);
        CirclePoint.Add(pos0);
        CirclePoint.Add(pos1);
        pos0 = new Vector2(twr - radius, thr - radius);//右上角圆心
        pos1 = new Vector2(twr, thr - radius);
        CirclePoint.Add(pos0);
        CirclePoint.Add(pos1);
        pos0 = new Vector2(twr - radius, -thr + radius);//右下角圆心
        pos1 = new Vector2(twr - radius, -thr);
        CirclePoint.Add(pos0);
        CirclePoint.Add(pos1);
        float degreeDelta = (float)(Mathf.PI / 2 / TriangleNum);//每一份等腰三角形的角度 默认6份
        List<float> degreeDeltaList = new List<float>() { Mathf.PI, Mathf.PI / 2, 0, (float)3 / 2 * Mathf.PI };

        for (int j = 0; j < CirclePoint.Count; j += 2)
        {
            float curDegree = degreeDeltaList[j / 2];//当前的角度
            AddVert(CirclePoint[j], tw, th, vh, vert);//添加扇形区域所有三角形公共顶点
            int thrdIndex = vh.currentVertCount;//当前三角形第二顶点索引
            int TriangleVertIndex = vh.currentVertCount - 1;//一个扇形保持不变的顶点索引
            List<Vector2> pos2List = new List<Vector2>();
            for (int i = 0; i < TriangleNum; i++)
            {
                curDegree += degreeDelta;
                if (pos2List.Count == 0)
                {
                    AddVert(CirclePoint[j + 1], tw, th, vh, vert);
                }
                else
                {
                    vert.position = pos2List[i - 1];
                    vert.uv0 = new Vector2(pos2List[i - 1].x + 0.5f, pos2List[i - 1].y + 0.5f);
                }
                pos2 = new Vector2(CirclePoint[j].x + radius * Mathf.Cos(curDegree), CirclePoint[j].y + radius * Mathf.Sin(curDegree));
                AddVert(pos2, tw, th, vh, vert);
                vh.AddTriangle(TriangleVertIndex, thrdIndex, thrdIndex + 1);
                thrdIndex++;
                pos2List.Add(vert.position);
            }
        }
    }
    protected Vector2[] GetTextureUVS(Vector2[] vhs, float tw, float th)
    {
        int count = vhs.Length;
        Vector2[] uvs = new Vector2[count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vhs[i].x / tw + 0.5f, vhs[i].y / th + 0.5f);//矩形的uv坐标  因为uv坐标原点在左下角，vh坐标原点在中心 所以这里加0.5（uv取值范围0~1）
        }
        return uvs;
    }
    protected void AddVert(Vector2 pos0, float tw, float th, VertexHelper vh, UIVertex vert)
    {
        vert.position = pos0;
        vert.uv0 = GetTextureUVS(new[] { new Vector2(pos0.x, pos0.y) }, tw, th)[0];
        vh.AddVert(vert);
    }
}
