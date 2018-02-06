using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grass
{
    public class TerrainGrassTile : MonoBehaviour
    {
        public List<TerrainGrass> grassList = new List<TerrainGrass>();//当前格子内的草
        public Rect rect;
        public Vector3 center;
        public Vector3 size;
        public void Awake()
        {
        }

        public void UpdateBox(float top, float bottom)
        {
            size = new Vector3(rect.width, top - bottom, rect.height);
            center = new Vector3(rect.xMin, bottom, rect.yMin) + size / 2;
        }

        public Vector3 Center { get { return center; } }

        public void ClearGrass()
        {
            grassList.Clear();
        }

        public void AddGrass(TerrainGrass grass)
        {
            grassList.Add(grass);
        }

        public void AddDisturbance(Vector3 pos, float radius, float force)
        {
            for (int i = 0; i < grassList.Count; i++) {
                if (grassList[i].gameObject.activeSelf) {
                    grassList[i].AddDisturbance(pos, radius, force);
                    grassList[i].UpdateMesh();
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.identity;
            Color oldColor = Gizmos.color;
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawWireCube(center, size);
            Gizmos.color = oldColor;
        }
    }

}