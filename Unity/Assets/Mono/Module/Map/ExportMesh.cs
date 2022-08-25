using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AmazingAssets.TerrainToMesh;
namespace ET
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ExportMesh : MonoBehaviour
    {
        public TerrainData terrainData;

        public int vertexCountHorizontal = 100;
        public int vertexCountVertical = 100;

        public void Generic()
        {
            if (terrainData == null)
                return;


            //1. Export mesh from terrain////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
           
            Mesh terrainMesh = terrainData.TerrainToMesh().ExportMesh(vertexCountHorizontal, vertexCountVertical, Normal.CalculateFromMesh);

            GetComponent<MeshFilter>().sharedMesh = terrainMesh;




            //2. Create material////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string shaderName = Utilities.GetUnityDefaultShader(); //Default shader based on used render pipeline

            Material material = new Material(Shader.Find(shaderName));

            GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
