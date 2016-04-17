using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TerrainTool
{
    [ExecuteInEditMode]
    public class TerrainTool : MonoBehaviour, ISerializationCallbackReceiver
    {
        [HideInInspector]
        public List<NodeContainer> nodeContainers = new List<NodeContainer>();
        public Terrain target;
        public GameObject entityContainer;
        public int seed;
        public int baseX, baseY;
        public void OnEnable()
        {
            if (nodeContainers.Count == 0) {
                NodeContainer height = new NodeContainer();
                height.SetNode(NodeMaker.Instance.CreateNode(NodeType.Output));
                height.rect = new Rect(-200, -120, 128, 128 + 16);
                nodeContainers.Add(height);
                NodeContainer perlin = new NodeContainer();
                perlin.SetNode(NodeMaker.Instance.CreateNode(NodeType.Generator, "Perlin Noise"));
                perlin.rect = new Rect(100, -120, 128, 128 + 16);
                nodeContainers.Add(perlin);
                height.node.inputs[0] = perlin.node;
            }
        }

        public void Sort()
        {
            nodeContainers.Sort((NodeContainer item0, NodeContainer item1) => {
                return item0.GetSortValue() - item1.GetSortValue();
            });
        }

        public NodeContainer Find(string name)
        {
            return nodeContainers.Find((NodeContainer n) => n.node.name == name);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
        }

        public void DoGenerate()
        {
            float startTime = Time.realtimeSinceStartup;
            if (target != null) {
                updateTerrainHeight(target);
                updateTerrainTexture(target);
                updateTerrainGrass(target);
                updateTerrainTree(target);
                updateTerrainEntity(target);
            }
            Debug.Log("generate terrain time = " + (Time.realtimeSinceStartup - startTime));
        }

        public void updateTerrainHeight(Terrain terr)
        {
            if (terr != null) {
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is HeightOutput) {
                        int w = terr.terrainData.heightmapWidth;
                        int h = terr.terrainData.heightmapHeight;
                        float realWidth = terr.terrainData.size.x;
                        float realHeight = terr.terrainData.size.z;
                        float[,] values = nodeContainers[i].node.update(seed, w, h, new Rect(baseX,baseY,realWidth,realHeight));
                        terr.terrainData.SetHeights(0, 0, values);
                        break;
                    }
                }
            }
        }

        public void updateTerrainTexture(Terrain terr)
        {
            if (terr != null) {
                List<SplatPrototype> prototypes = new List<SplatPrototype>();
                List<TextureOutput> textureOutputs = new List<TextureOutput>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is TextureOutput) {
                        textureOutputs.Add((TextureOutput)nodeContainers[i].node);
                    }
                }
                textureOutputs.Sort((TextureOutput left, TextureOutput right) => { return left.paintOrder - right.paintOrder; });
                for (int i = 0; i < textureOutputs.Count; i++) {
                    TextureOutput texNode = textureOutputs[i];
                    if (texNode.texture != null) {
                        SplatPrototype p = new SplatPrototype();
                        p.texture = texNode.texture;
                        p.normalMap = texNode.normal;
                        p.tileSize = new Vector2(texNode.texSizeX, texNode.texSizeY);
                        prototypes.Add(p);
                    }
                }
                terr.terrainData.splatPrototypes = prototypes.ToArray();
                int w = terr.terrainData.alphamapWidth;
                int h = terr.terrainData.alphamapHeight;
                float realWidth = terr.terrainData.size.x;
                float realHeight = terr.terrainData.size.z;
                float[,] totalAlpha = new float[w, h];
                int layers = terr.terrainData.alphamapLayers;
                float[,,] alphaDatas = new float[w, h, layers];
                for (int ly = layers - 1; ly >= 0; ly--) {
                    NodeBase tempNode = null;
                    for (int i = 0; i < textureOutputs.Count; i++) {
                        if (textureOutputs[i].paintOrder == ly) {
                            tempNode = textureOutputs[i];
                        }
                    }
                    if (tempNode != null) {
                        float[,] values = tempNode.update(seed, w, h, new Rect(baseX, baseY, realWidth, realHeight));
                        for (int i = 0; i < w; i++) {
                            for (int j = 0; j < h; j++) {
                                if (totalAlpha[i, j] + values[i, j] <= 1.00001f) {
                                    alphaDatas[i, j, ly] = values[i, j];
                                    totalAlpha[i, j] += values[i, j];
                                }
                                else {
                                    alphaDatas[i, j, ly] = 1f - totalAlpha[i, j];
                                    totalAlpha[i, j] = 1f;
                                }
                            }
                        }
                    }
                }
                terr.terrainData.SetAlphamaps(0, 0, alphaDatas);
            }
        }

        public void updateTerrainGrass(Terrain terr)
        {
            if (terr != null) {
                List<DetailPrototype> detailList = new List<DetailPrototype>();
                List<GrassOutput> grassOutputs = new List<GrassOutput>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is GrassOutput) {
                        GrassOutput grassNode = nodeContainers[i].node as GrassOutput;
                        DetailPrototype p = new DetailPrototype();
                        p.prototypeTexture = grassNode.texture;
                        p.renderMode = grassNode.bBillboard ? DetailRenderMode.GrassBillboard : DetailRenderMode.Grass;
                        detailList.Add(p);
                        grassOutputs.Add(grassNode);
                    }
                }
                terr.terrainData.detailPrototypes = detailList.ToArray();
                int w = terr.terrainData.detailWidth;
                int h = terr.terrainData.detailHeight;
                float realWidth = terr.terrainData.size.x;
                float realHeight = terr.terrainData.size.z;
                int layers = terr.terrainData.detailPrototypes.Length;
                for (int layer = 0; layer < grassOutputs.Count; layer++) {
                    GrassOutput texNode = grassOutputs[layer];
                    float[,] values = texNode.update(seed, w, h, new Rect(baseX, baseY, realWidth, realHeight));
                    int[,] detailValues = new int[w, h];
                    for (int x = 0; x < w; x++) {
                        for (int y = 0; y < h; y++) {
                            detailValues[x, y] = (int)(values[x, y] * values[x, y] * 16);
                        }
                    }
                    if (layer < layers) {
                        terr.terrainData.SetDetailLayer(0, 0, layer, detailValues);
                    }
                }
            }
        }

        private List<TreeInstance> GetTreeInstance(Terrain terr, bool bEntity)
        {
            int terrainWidth = terr.terrainData.heightmapWidth;
            int terrainHeight = terr.terrainData.heightmapHeight;
            float realWidth = terr.terrainData.size.x;
            float realHeight = terr.terrainData.size.z;

            float pixelSize = terr.terrainData.size.x / terrainWidth;

            List<TreeInstance> instances = new List<TreeInstance>();
            for (int n = 0; n < nodeContainers.Count; n++) {
                if (nodeContainers[n].node is TreeOutput) {
                    TreeOutput treeNode = nodeContainers[n].node as TreeOutput;
                    if (bEntity == treeNode.isEntity) {
                        float[,] values = treeNode.update(seed, terrainWidth, terrainHeight, new Rect(baseX, baseY, realWidth, realHeight));
                        List<Vector3> treePos = new List<Vector3>();
                        List<float> treeAngles = new List<float>();
                        List<float> treeScales = new List<float>();
                        List<int> prefabIndexs = new List<int>();
                        //TreePrototype prot = terr.terrainData.treePrototypes[texNode.treeIndex];
                        for (int x = 0; x < terrainWidth; x++) {
                            for (int y = 0; y < terrainHeight; y++) {
                                int treeIndexForHash = treeNode.startTreePropertyIndex + (bEntity ? 999 : 0);
                                int treeNum = treeNode.getTreeNum(baseX + x, baseY + y, values[x, y], treeNode.density, treeIndexForHash);
                                for (int t = 0; t < treeNum; t++) {
                                    Vector2 offset = (treeNode.getTreePos(baseX + x, baseY + y, t, pixelSize * 2f, treeIndexForHash));
                                    Vector2 pos = new Vector2(x + offset.x, y + offset.y);//翻转x,y.
                                    float height = terr.terrainData.GetInterpolatedHeight(pos.x / terrainWidth, pos.y / terrainHeight) / terr.terrainData.size.y;
                                    Vector3 newPos = new Vector3(pos.x / terrainWidth, height, pos.y / terrainHeight);
                                    treePos.Add(newPos);
                                    treeAngles.Add(treeNode.GetAngle(baseX + x, baseY + y, t, pixelSize * 2f, treeIndexForHash));
                                    treeScales.Add(treeNode.GetScale(baseX + x, baseY + y, t, pixelSize * 2f, treeIndexForHash));
                                    prefabIndexs.Add(treeNode.GetPrefabIndex(baseX + x, baseY + y, t, pixelSize * 2f, treeIndexForHash));
                                }
                            }
                        }
                        //
                        for (int i = 0; i < treePos.Count; i++) {
                            TreeInstance ins = new TreeInstance();
                            ins.position = treePos[i];
                            ins.prototypeIndex = treeNode.startTreePropertyIndex + prefabIndexs[i];
                            ins.color = Color.white;
                            ins.lightmapColor = Color.white;
                            float s = Mathf.Lerp(treeNode.minSize, treeNode.maxSize, treeScales[i]);
                            ins.heightScale = s;
                            ins.widthScale = s;
                            ins.rotation = 360 * treeAngles[i];
                            instances.Add(ins);
                        }
                    }
                }
            }

            return instances;
        }

        public void updateTerrainTree(Terrain terr)
        {
            if (terr != null) {
                List<TreePrototype> treeList = new List<TreePrototype>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is TreeOutput) {
                        TreeOutput treeNode = (TreeOutput)(nodeContainers[i].node);
                        treeNode.startTreePropertyIndex = treeList.Count;
                        for (int p = 0; p < treeNode.prefabs.Length; p++) {
                            if (treeNode.prefabs[p] != null && !treeNode.isEntity) {//排除Entity
                                TreePrototype tp = new TreePrototype();
                                tp.prefab = treeNode.prefabs[p];
                                tp.bendFactor = treeNode.bendFactor;
                                treeList.Add(tp);
                            }
                        }
                    }
                }
                terr.terrainData.treePrototypes = treeList.ToArray();

                List<TreeInstance> instances = GetTreeInstance(terr, false);

                terr.terrainData.treeInstances = instances.ToArray();
            }
        }

        public void updateTerrainEntity(Terrain terr,Func<GameObject, GameObject> funNewObj = null)
        {
            if (terr != null) {
                List<TreePrototype> treeList = new List<TreePrototype>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is TreeOutput) {
                        TreeOutput treeNode = (TreeOutput)(nodeContainers[i].node);
                        treeNode.startTreePropertyIndex = treeList.Count;
                        for (int p = 0; p < treeNode.prefabs.Length; p++) {
                            if (treeNode.prefabs[p] != null && treeNode.isEntity) {///选取所有Entity
                                TreePrototype tp = new TreePrototype();
                                tp.prefab = treeNode.prefabs[p];
                                tp.bendFactor = treeNode.bendFactor;
                                treeList.Add(tp);
                            }
                        }
                    }
                }

                GameObject container = entityContainer;
                if (container == null) {
                    container = new GameObject("auto_tree");
                }

                {
                    Transform[] childs = container.GetComponentsInChildren<Transform>();
                    foreach (Transform child in childs) {
                        if (child != container.transform) {
                            if (child != null) {
                                GameObject.DestroyImmediate(child.gameObject);
                            }
                        }
                    }
                }


                List<TreeInstance> instances = GetTreeInstance(terr, true);

                for (int i = 0; i < instances.Count; i++) {
                    GameObject prefab = treeList[instances[i].prototypeIndex].prefab;
                    GameObject obj = (funNewObj == null) ? GameObject.Instantiate(prefab) : funNewObj(prefab);
                    obj.transform.SetParent(container.transform, true);
                    obj.transform.position = Vector3.Scale(instances[i].position, terr.terrainData.size) + container.transform.position;
                    obj.transform.rotation = prefab.transform.rotation * Quaternion.AngleAxis(instances[i].rotation, Vector3.up);
                    obj.transform.localScale = new Vector3(instances[i].widthScale, instances[i].heightScale, instances[i].widthScale);
                    obj.transform.name = prefab.name;
                }
            }
        }
    }
}
