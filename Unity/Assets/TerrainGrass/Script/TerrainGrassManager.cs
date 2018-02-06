using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Grass
{
    public class TerrainGrassManager : MonoBehaviour
    {
        public bool soft;
        public float grassViewDistance = 30;
        public int resolutionPerPatch = 16;
        [Range(0f, 0.5f)]
        public float wind = 0.02f;
        [Range(0, 10f)]
        public float frequency = 3;
        [Range(0, 2f)]
        public float maxAngle = 0.6f;
        [Range(0, 0.5f)]
        public float waveStrength = 0.05f;
        [Range(0.3f, 10)]
        public float waveLenght = 3f;
        [Range(0.2f, 10)]
        public float waveFrequency = 1f;
        int rollNum;
        Terrain terrain;
        TerrainData terrainData;
        public Material material;

        Pool<TerrainGrass> meshPool;
        Pool<TerrainGrassTile> tilePool;
        RollMap<TerrainGrassTile> rollMap;
        List<TerrainGrassTile> taskList = new List<TerrainGrassTile>();
        Material[] materials;
        DetailPrototype[] details;
        Transform recyle;//回收节点
        List<short[,]> detailDensity = new List<short[,]>();

        bool[,,] patchVisible;//这个点是否有某种类型的草
        private List<object> subscribes = new List<object>();

        public static TerrainGrassManager Instance { get { return m_instance; } }
        static TerrainGrassManager m_instance;

        public void OnEnable() {
            m_instance = this;
#if QINSHI
            subscribes.Add(Utility.EventSystem.Subscribe<Vector3, float, float>("add_disturbance", "grass", AddDisturbance));
#endif
            terrain = GetComponent<Terrain>();
            terrainData = terrain.terrainData;
            terrain.detailObjectDistance = 0;

            UpdatePatchVisible();

            recyle = new GameObject("recyle").transform;
            recyle.SetParent(transform, false);

            details = terrain.terrainData.detailPrototypes;
            materials = new Material[details.Length];
            for (int i = 0; i < details.Length; i++) {
                materials[i] = new Material(material);// new Material(shader);
                materials[i].mainTexture = details[i].prototypeTexture;
                materials[i].SetFloat("_GrassMinWidth", details[i].minWidth);
                materials[i].SetFloat("_GrassMaxWidth", details[i].maxWidth);
                materials[i].SetFloat("_GrassMinHeight", details[i].minHeight);
                materials[i].SetFloat("_GrassMaxHeight", details[i].maxHeight);
                materials[i].SetColor("_WavingTint", terrain.terrainData.wavingGrassTint);
                materials[i].SetColor("_GrassColor", (details[i].dryColor + details[i].healthyColor) * 0.5f);
                materials[i].SetFloat("_ViewDistance", grassViewDistance);
                materials[i].SetVector("_TerrainOffsetSize", new Vector4(terrain.transform.position.x, terrain.transform.position.z, terrainData.size.x, terrainData.size.z));
                if (terrain.lightmapIndex >= 0 && terrain.lightmapIndex < LightmapSettings.lightmaps.Length) {
                    materials[i].SetTexture("_Lightmap", LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor);
                }

                int[,] density = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailResolution, terrain.terrainData.detailResolution, i);
                short[,] sDensity = new short[density.GetLength(0), density.GetLength(1)];
                for (int x = 0; x < density.GetLength(0); x++) {
                    for (int y = 0; y < density.GetLength(1); y++) {
                        sDensity[x,y] = (short)density[x, y];
                    }
                }
                detailDensity.Add(sDensity);
            }



            UpdateGrassShaderParams();

            meshPool = new Pool<TerrainGrass>();
            meshPool.funNew = () => {
                GameObject obj = new GameObject("grass");
                obj.transform.SetParent(transform);
                //obj.layer = terrain.gameObject.layer;
                TerrainGrass grass = obj.AddComponent<TerrainGrass>();
                grass.Init(terrain, soft);
                return grass;
            };

            tilePool = new Pool<TerrainGrassTile>();
            tilePool.funNew = () => {
                GameObject obj = new GameObject("tile");
                obj.transform.SetParent(transform);
                TerrainGrassTile tile = obj.AddComponent<TerrainGrassTile>();
                return tile;
            };

            rollNum = (int)((grassViewDistance * 2 - 0.1f) / PatchSize) + 1;

            rollMap = new RollMap<TerrainGrassTile>(rollNum, rollNum, -10000, -10000);
            rollMap.funDelete = (TerrainGrassTile tile) => {
                if (tile) {
                    tile.gameObject.SetActive(false);
                    tile.transform.SetParent(recyle, false);
                    tilePool.Recycle(tile);
                    for (int i = 0; i < tile.grassList.Count; i++) {
                        meshPool.Recycle(tile.grassList[i]);
                        tile.transform.SetParent(recyle, false);
                    }
                    taskList.RemoveAll((TerrainGrassTile item) => item == tile);
                }
            };
            rollMap.funNew = CreateNode;

            if (terrain.lightmapIndex >= 0 && terrain.lightmapIndex < LightmapSettings.lightmaps.Length) {
                Shader.SetGlobalTexture("_GrassLightmap", LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor);
            }
        }

        public void OnDisable() {
#if QINSHI
            for (int i = 0; i < subscribes.Count; i++) {
                Utility.EventSystem.Unsubscribe(subscribes[i]);
            }
#endif
            subscribes.Clear();

            TerrainGrassTile[,] tile = rollMap.GetAll();
            for (int i = 0; i < tile.GetLength(0); i++) {
                for (int j = 0; j < tile.GetLength(1); j++) {
                    if (tile[i, j] != null) {
                        GameObject.Destroy(tile[i, j].gameObject);
                    }
                }
            }
            rollMap = null;
            for (int i = 0; i < tilePool.objects.Count; i++) {
                GameObject.Destroy(tilePool.objects[i].gameObject);
            }
            tilePool = null;
            meshPool = null;
            GameObject.Destroy(recyle.gameObject);
            recyle = null;
            taskList.Clear();
        }

        //新滚动出来的格子
        TerrainGrassTile CreateNode(int x, int y) {
            if (x >= 0 && y >= 0 && x < PatchNumX && y < PatchNumX) {
                bool anyLayer = false;
                for (int l = 0; l < details.Length; l++) {
                    if (patchVisible[x, y, l]) {
                        anyLayer = true;
                    }
                }

                if (anyLayer) {
                    TerrainGrassTile tile = tilePool.New();//从池子里申请一个
                    tile.transform.SetParent(transform, false);
                    tile.rect = new Rect(x * PatchSize, y * PatchSize, PatchSize, PatchSize);
                    tile.grassList.Clear();
                    for (int l = 0; l < details.Length; l++) {
                        if (patchVisible[x, y, l]) {
                            TerrainGrass grass = meshPool.New();
                            grass.transform.SetParent(tile.transform, false);
                            grass.transform.localPosition = Vector3.zero;
                            grass.Reset(materials[l], l);
                            grass.rect = tile.rect;
                            grass.patchX = x * resolutionPerPatch;
                            grass.patchY = y * resolutionPerPatch;
                            grass.resolutionPerPatch = resolutionPerPatch;
                            grass.detailDensity = detailDensity;
                            tile.AddGrass(grass);
                        }
                    }
                    taskList.Add(tile);
                    return tile;
                }
            }
            return null;
        }

        int PatchNumX { get { return terrainData.detailResolution / resolutionPerPatch; } }

        float PatchSize { get { return terrain.terrainData.size.x * resolutionPerPatch / terrainData.detailResolution; } }

        static int GetNearestTaskIndex(Vector3 centerPos, List<TerrainGrassTile> taskList) {
            float minDist = 1e10f;
            int minIndex = -1;
            for (int i = 0; i < taskList.Count; i++) {
                float d = (new Vector2(centerPos.x, centerPos.z) - taskList[i].rect.center).sqrMagnitude;
                if (d < minDist) {
                    minDist = d;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        void UpdateGrassShaderParams() {
            for (int i = 0; i < materials.Length; i++) {
                materials[i].SetFloat("_MaxRotAngle", maxAngle);
                materials[i].SetFloat("_WindRotAngle", wind);
                materials[i].SetFloat("_WaveStrength", waveStrength);
                materials[i].SetFloat("_RotSpeed", frequency);
                materials[i].SetFloat("_WaveSpeed", waveFrequency);
                materials[i].SetFloat("_WaveLength", waveLenght);
            }
        }

        void Update() {
            Vector3 centerPos = Vector3.zero;
            bool bPosValid = false;
            if (Camera.main) {
                centerPos = Camera.main.transform.position;
                bPosValid = true;
            }
#if QINSHI
            if (CameraManager.Instance.controller.GetFollowTargetObject() != null) {
                centerPos = CameraManager.Instance.controller.GetFollowTargetObject().transform.position;
                bPosValid = true;
            }
#endif

            if (bPosValid) {
                int currentX = GetGridStartIndex(centerPos.x);
                int currentY = GetGridStartIndex(centerPos.z);//摄像机当前所处的格子
                rollMap.RollTo(currentX, currentY);

                if (taskList.Count > 0) {
                    int nearestIndex = GetNearestTaskIndex(centerPos, taskList);
                    TerrainGrassTile tile = taskList[nearestIndex];
                    taskList.RemoveAt(nearestIndex);
                    float top = 0;
                    float bottom = 10000;

                    tile.gameObject.SetActive(true);
                    for (int i = 0; i < tile.grassList.Count; i++) {
                        TerrainGrass grass = tile.grassList[i];
                        bool hasMesh = grass.CreateMesh();
                        if (hasMesh) {
                            grass.gameObject.SetActive(true);
                            top = Mathf.Max(grass.top, top);
                            bottom = Mathf.Min(grass.bottom, bottom);
                        }
                    }
                    tile.UpdateBox(top, bottom);

                    //正在生成时，慢慢的修改可视距离，减小跳动
                    for (int i = 0; i < materials.Length; i++) {
                        materials[i].SetFloat("_ViewDistance", (tile.Center - centerPos).magnitude - PatchSize / 2.0f);
                    }
                }
            }
            Shader.SetGlobalFloat("_GrassTime", Time.time);

#if UNITY_EDITOR
            UpdateGrassShaderParams();
#endif
        }

        int GetGridStartIndex(float pos_value) {
            return (int)(pos_value / PatchSize + (0.5f - (rollNum % 2) / 2)) - rollNum / 2;
        }

        int GetGridIndex(float pos_value) {
            return (int)(pos_value / PatchSize);
        }

        void UpdatePatchVisible() {
            float startTime = Time.realtimeSinceStartup;
            float density = terrain.detailObjectDensity;
            DetailPrototype[] details = terrain.terrainData.detailPrototypes;
            patchVisible = new bool[PatchNumX, PatchNumX, details.Length];
            for (int l = 0; l < details.Length; l++) {
                if (details[l].renderMode == DetailRenderMode.GrassBillboard) {
                    int[,] nums = terrain.terrainData.GetDetailLayer(0, 0, terrainData.detailResolution, terrainData.detailResolution, l);
                    for (int i = 0; i < PatchNumX; i++) {
                        for (int j = 0; j < PatchNumX; j++) {
                            bool visible = false;
                            for (int x = 0; x < resolutionPerPatch; x++) {
                                for (int y = 0; y < resolutionPerPatch; y++) {
                                    int xx = j * resolutionPerPatch + x;
                                    int yy = i * resolutionPerPatch + y;
                                    //int num = GrassTool.GetNewCount(nums[xx, yy], density, x, y);
                                    if (nums[xx, yy] > 0) {
                                        visible = true;
                                        break;
                                    }
                                }
                                if (visible == true) {
                                    break;
                                }
                            }
                            patchVisible[i, j, l] = visible;
                        }
                    }
                }
            }
            //Debug.Log("UpdatePatchVisible Time:" + (Time.realtimeSinceStartup - startTime));
        }

        public void AddDisturbance(Vector3 pos, float radius, float force) {
            int minX = GetGridIndex(pos.x - radius);
            int maxX = GetGridIndex(pos.x + radius);
            int minY = GetGridIndex(pos.z - radius);
            int maxY = GetGridIndex(pos.z + radius);
            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    TerrainGrassTile tile = rollMap.Get(x, y);
                    if (tile) {
                        tile.AddDisturbance(pos, radius, force);
                    }
                }
            }
        }
    }

    class Pool<T> where T : new()
    {
        public delegate T FunNew();
        public FunNew funNew;
        public List<T> objects = new List<T>();
        public T New() {
            if (objects.Count > 0) {
                T obj = objects[0];
                objects.RemoveAt(0);
                return obj;
            } else {
                if (funNew != null) {
                    return funNew();
                } else {
                    return new T();
                }
            }
        }
        public void Recycle(T obj) {
            if (obj != null) {
                objects.Add(obj);
            }
        }
    }

    class RollMap<T> where T : new()
    {
        public delegate T FunNew(int x, int y);
        public FunNew funNew;//新建
        public System.Action<T> funDelete;//删除

        T[,] array;
        int numX, numY;
        int startX, startY;//左下角的格子坐标
        public RollMap(int numX, int numY, int curX, int curY) {
            array = new T[numX, numY];
            this.numX = numX;
            this.numY = numY;
            startX = curX;
            startY = curY;
        }

        public T Get(int x, int y) {
            x -= startX;
            y -= startY;
            if (x >= 0 && x < numX && y >= 0 && y < numY) {
                return array[x, y];
            }
            return default(T);
        }

        public T[,] GetAll() {
            return array;
        }

        public void RollTo(int newX, int newY) {
            int rollNumX = Mathf.Abs(newX - startX);
            int rollNumY = Mathf.Abs(newY - startY);

            if (rollNumX >= numX || rollNumY >= numY) {
                for (int i = 0; i < numX; i++) {
                    for (int j = 0; j < numY; j++) {
                        funDelete(array[i, j]);
                    }
                }
                for (int i = 0; i < numX; i++) {
                    for (int j = 0; j < numY; j++) {
                        array[i, j] = funNew(i + newX, j + newY);
                    }
                }
            } else {
                if (rollNumX != 0) {//x方向发生滚动
                    for (int j = 0; j < numY; j++) {
                        if (newX > startX) {//超前滚
                            for (int r = 0; r < rollNumX; r++) {
                                funDelete(array[0, j]);
                                for (int i = 0; i < numX - 1; i++) {
                                    array[i, j] = array[i + 1, j];
                                }
                                array[numX - 1, j] = funNew(startX + numX + r, j + startY);
                            }
                        } else {//朝后滚
                            for (int r = 0; r < rollNumX; r++) {
                                funDelete(array[numX - 1, j]);
                                for (int i = numX - 1; i >= 1; i--) {
                                    array[i, j] = array[i - 1, j];
                                }
                                array[0, j] = funNew(startX - 1 - r, j + startY);
                            }
                        }
                    }
                }

                if (rollNumY != 0) {//y方向发生滚动
                    for (int i = 0; i < numX; i++) {
                        if (newY > startY) {//超前滚
                            for (int r = 0; r < rollNumY; r++) {
                                funDelete(array[i, 0]);
                                for (int j = 0; j < numY - 1; j++) {
                                    array[i, j] = array[i, j + 1];
                                }
                                array[i, numY - 1] = funNew(i + newX, startY + numY + r);
                            }
                        } else {//朝后滚
                            for (int r = 0; r < rollNumY; r++) {
                                funDelete(array[i, numY - 1]);
                                for (int j = numY - 1; j >= 1; j--) {
                                    array[i, j] = array[i, j - 1];
                                }
                                array[i, 0] = funNew(i + newX, startY - 1 - r);
                            }
                        }
                    }
                }
            }

            startX = newX;
            startY = newY;
        }
    }
}