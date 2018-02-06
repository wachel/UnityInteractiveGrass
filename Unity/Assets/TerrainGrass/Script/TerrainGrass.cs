using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Grass
{
    public class TerrainGrass : MonoBehaviour
    {
        public int patchX;
        public int patchY;
        public int resolutionPerPatch;
        public List<short[,]> detailDensity;
        public Rect rect;

        public MeshFilter filter;
        public new MeshRenderer renderer;
        Terrain terrain;
        GrassMesh grassMesh = new GrassMesh();
        int layer;
        public float top, bottom;
        public bool AddForce = false;

        public void Awake()
        {
            filter = GetComponent<MeshFilter>();
            if (filter == null) {
                filter = gameObject.AddComponent<MeshFilter>();
            }
            renderer = GetComponent<MeshRenderer>();
            if (renderer == null) {
                renderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        //public void Update()
        //{
        //    if (AddForce) {
        //        AddForce = false;
        //        AddDisturbance(new Vector3(rect.center.x, (top + bottom) * 0.5f, rect.center.y),1,1);
        //        UpdateMesh();
        //    }
        //}   

        public void Init(Terrain terrain,bool soft)
        {
            this.terrain = terrain;
            //renderer.lightmapIndex = terrain.lightmapIndex;
            //renderer.lightmapScaleOffset = terrain.lightmapScaleOffset;
            grassMesh.doubleQuad = soft;
        }

        public void Reset(Material mat, int layer)
        {
            renderer.material = mat;
            this.layer = layer;
        }

        public bool CreateMesh()
        {
            UnityEngine.Profiling.Profiler.BeginSample("CreateMesh0");
            grassMesh.Clear();
            bottom = 1e10f;
            top = -1e10f;
            bool hasMesh = false;
            float globalDensity = terrain.detailObjectDensity;
            float pixelSize = terrain.terrainData.size.x / terrain.terrainData.detailResolution;
            //int[,] detailNums = terrain.terrainData.GetDetailLayer(patchX, patchY, resolutionPerPatch, resolutionPerPatch, layer);//导致GC
            for (int i = 0; i < resolutionPerPatch; i++) {
                for (int j = 0; j < resolutionPerPatch; j++) {
                    int num = GrassDither.GetNewCount(detailDensity[layer][patchY + i, patchX + j], globalDensity, j, i);
                    //int num = GrassDither.GetNewCount(detailNums[i, j], globalDensity, j, i);
                    for (int n = 0; n < num; n++) {
                        float x = rect.xMin + (j + Random.value) * pixelSize;
                        float y = rect.yMin + (i + Random.value) * pixelSize;
                        Vector3 pos = new Vector3(x, 0, y);
                        pos.y = terrain.SampleHeight(pos) + terrain.transform.position.y;

                        //Color light = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor.GetPixelBilinear(pos.x / terrain.terrainData.size.x, pos.z / terrain.terrainData.size.z);
                        grassMesh.AddQuad(pos);
                        bottom = bottom > pos.y ? pos.y : bottom;
                        top = top < pos.y ? pos.y : top;
                        hasMesh = true;
                    }
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();

            if (hasMesh) {

                top = top + 1f;
                grassMesh.CreateMesh();

                grassMesh.mesh.bounds = new Bounds(new Vector3(rect.center.x, (top + bottom) * 0.5f, rect.center.y), new Vector3(rect.width, top - bottom, rect.height));
                
                filter.mesh = grassMesh.mesh;
            } else {
                filter.mesh = null;
            }
            return hasMesh;
        }

        public void AddDisturbance(Vector3 pos,float radius,float force)
        {
            grassMesh.AddDisturbance(pos, radius,force);
        }

        public void UpdateMesh()
        {
            grassMesh.UpdateMesh();
        }

        void DrawGizmos(bool bSelected)
        {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.green;
            Vector3 size = new Vector3(rect.width, 0, rect.height);
            Gizmos.DrawWireCube(new Vector3(rect.x, 0, rect.y) + size * 0.5f, size * 0.9f);
            Gizmos.color = oldColor;
        }
        public void OnDrawGizmos()
        {
            //DrawGizmos(false);
        }

        public void OnDrawGizmosSelected()
        {
            //DrawGizmos(true);
        }
    }
}