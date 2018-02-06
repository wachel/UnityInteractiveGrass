using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grass
{
    public class GrassDither
    {
        // 8x8 dither table from http://en.wikipedia.org/wiki/Ordered_dithering
        static readonly float[] kDitherTable = {
            1, 49, 13, 61, 4, 52, 16, 64,
            33, 17, 45, 29, 36, 20, 48, 32,
            9, 57, 5, 53, 12, 60, 8, 56,
            41, 25, 37, 21, 44, 28, 40, 24,
            3, 51, 15, 63, 2, 50, 14, 62,
            35, 19, 47, 31, 34, 18, 46, 30,
            11, 59, 7, 55, 10, 58, 6, 54,
            43, 27, 39, 23, 42, 26, 38, 22,
        };
        public static int GetNewCount(int origCount, float density, int x, int y)
        {
            return (int)(origCount * density + (kDitherTable[(x & 7) * 8 + (y & 7)] - 0.5f) / 64.0f);
        }
    }

    public class GrassMesh
    {
        public Mesh mesh;
        public bool doubleQuad;

        static GrassMemoryPool pool = new GrassMemoryPool(4096, 4096 * 3 / 2);
        List<Vector3> posArray = new List<Vector3>(512);
        List<Vector4> tangentArray = new List<Vector4>(2048);

        public void Clear()
        {
            pool.Clear();
        }
        public void AddQuad(Vector3 pos)
        {
            int startIndex = pool.vertices.Count;

            pool.vertices.Add(pos);
            pool.vertices.Add(pos);
            pool.vertices.Add(pos);
            pool.vertices.Add(pos);
            if (doubleQuad) {
                pool.vertices.Add(pos);
                pool.vertices.Add(pos);
            }

            float angle = Random.Range(-Mathf.PI * 0.2f, Mathf.PI * 0.2f);
            float size = Random.value;
            pool.uvs.Add(new Vector4(0, 0, angle, size));
            pool.uvs.Add(new Vector4(1, 0, angle, size));
            if (doubleQuad) {
                pool.uvs.Add(new Vector4(0, 0.5f, angle, size));
                pool.uvs.Add(new Vector4(1, 0.5f, angle, size));
            }
            pool.uvs.Add(new Vector4(0, 1, angle, size));
            pool.uvs.Add(new Vector4(1, 1, angle, size));

            pool.indices.Add(startIndex + 0);
            pool.indices.Add(startIndex + 2);
            pool.indices.Add(startIndex + 3);
            pool.indices.Add(startIndex + 0);
            pool.indices.Add(startIndex + 3);
            pool.indices.Add(startIndex + 1);
            if (doubleQuad) {
                pool.indices.Add(startIndex + 2);
                pool.indices.Add(startIndex + 4);
                pool.indices.Add(startIndex + 5);
                pool.indices.Add(startIndex + 2);
                pool.indices.Add(startIndex + 5);
                pool.indices.Add(startIndex + 3);
            }

            pool.tangents.Add(new Vector4(0, 0, 0, 0));
            pool.tangents.Add(new Vector4(0, 0, 0, 0));
            pool.tangents.Add(new Vector4(0, 0, 0, 0));
            pool.tangents.Add(new Vector4(0, 0, 0, 0));
            if (doubleQuad) {
                pool.tangents.Add(new Vector4(0, 0, 0, 0));
                pool.tangents.Add(new Vector4(0, 0, 0, 0));
            }

            //for (int i = 0; i < (doubleQuad ? 6 : 4); i++) {
            //    pool.colors.Add(light);
            //}
        }
        

        public void CreateMesh()
        {
            posArray.Clear();
            int vertexCountPerGrass = doubleQuad ? 6 : 4;
            for (int i = 0; i < pool.vertices.Count / vertexCountPerGrass; i++) {
                posArray.Add(pool.vertices[i * vertexCountPerGrass]);
            }

            mesh = new Mesh();
            mesh.SetVertices(pool.vertices);
            mesh.SetUVs(0, pool.uvs);
            mesh.SetTriangles(pool.indices, 0);
            mesh.SetTangents(pool.tangents);
            mesh.MarkDynamic();
        }

        //模拟shader中的算法，得到当前摇摆幅度，此代码需要与shader中保持一致
        public static float Simulate(Vector4 tangent, float time)
        {
            if (time < tangent.w) {//上升期
                float t = (time - tangent.z) / (tangent.w - tangent.z);
                return Mathf.Lerp(tangent.x, tangent.y, Mathf.Pow(t, 0.5f));
            } else {
                float duration = 4;//下降时长
                float t = (time - tangent.w) / (duration);
                return Mathf.Lerp(tangent.y, 0, t);
            }
        }

        public void AddDisturbance(Vector3 pos,float radius,float forceFactor = 1.0f)
        {
            int vertexCountPerGrass = doubleQuad ? 6 : 4;
            if (mesh) {
                mesh.GetTangents(tangentArray);

                float delatTime = Time.deltaTime;
                for (int i = 0; i < posArray.Count; i++) {
                    //float force = 1f / ((posArray[i] - pos).sqrMagnitude + 0.01f);
                    //force = Mathf.Min(force, 1);
                    float r2 = (posArray[i] - pos).magnitude;
                    if (r2 < radius) {
                        float force = (1 - r2 / radius) * forceFactor;
                        
                        Vector4 old = tangentArray[i * vertexCountPerGrass];//x:初始值，y:最大值，z:开始时间，w:最大值时间
                        float curValue = GrassMesh.Simulate(old, Time.time);
                        Vector4 newTangent = new Vector4(curValue, Mathf.Max(curValue, force), Time.time, Time.time + 0.2f);//x:初始值，y:最大值，z:开始时间，w:最大值时间
                        for (int t = 0; t < vertexCountPerGrass; t++) {
                            tangentArray[i * vertexCountPerGrass + t] = newTangent;
                        }
                    }
                }
            }
        }

        public void UpdateMesh()
        {
            if (mesh) {
                mesh.SetTangents(tangentArray);
            }
        }
    }

    public class GrassMemoryPool
    {
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<int> indices;
        public List<Vector4> uvs;
        public List<Vector4> tangents;
        public List<Color> colors;

        public GrassMemoryPool(int verticesNum, int indicesNum)
        {
            vertices = new List<Vector3>(verticesNum);
            normals = new List<Vector3>(verticesNum);
            indices = new List<int>(indicesNum);
            uvs = new List<Vector4>(verticesNum);
            tangents = new List<Vector4>(verticesNum);
            colors = new List<Color>(verticesNum);
        }

        public void Clear()
        {
            vertices.Clear();
            normals.Clear();
            indices.Clear();
            uvs.Clear();
            tangents.Clear();
            colors.Clear();
        }
    }
}