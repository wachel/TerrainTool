using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace TerrainTool
{
    [ExecuteInEditMode]
    public class TerrainTool : MonoBehaviour , ISerializationCallbackReceiver
    {
        [HideInInspector]
        public List<NodeContainer> nodeContainers = new List<NodeContainer>();
        public int seed;
        public int baseX, baseY;
        public void OnEnable()
        {
            if (nodeContainers.Count == 0) {
                {
                    NodeContainer temp = new NodeContainer();
                    temp.name = "Height Output";
                    temp.SetNode(NodeMaker.Instance.CreateNode(NodeType.Output));
                    temp.rect = new Rect(0, 0, 128, 128 + 16);
                    nodeContainers.Add(temp);
                }
                {
                    NodeContainer temp = new NodeContainer();
                    temp.name = "Const Value";
                    temp.SetNode(NodeMaker.Instance.CreateNode(NodeType.Generator));
                    temp.rect = new Rect(100, 200, 128, 128 + 16);
                    nodeContainers.Add(temp);
                }
            }
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
            Terrain terr = Terrain.activeTerrain;
            if (terr != null) {
                updateTerrainHeight();
                updateTerrainTexture();
                updateTerrainGrass();
                updateTerrainTree();
                updateTerrainEntity();
            }
            Debug.Log("generate terrain time = " + (Time.realtimeSinceStartup - startTime));
        }

        public void updateTerrainHeight()
        {
            Terrain terr = Terrain.activeTerrain;
            if (terr != null) {
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is HeightOutput) {
                        int w = terr.terrainData.heightmapWidth;
                        int h = terr.terrainData.heightmapHeight;
                        float[,] values = nodeContainers[i].node.update(seed, baseX, baseY, w, h);
                        terr.terrainData.SetHeights(0, 0, values);
                        break;
                    }
                }
            }
        }

        public void updateTerrainTexture()
        {
            Terrain terr = Terrain.activeTerrain;
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
                        texNode.textureIndex = prototypes.Count;
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
                float[,] totalAlpha = new float[w, h];
                float scaleX = terr.terrainData.heightmapWidth / ((float)w);
                float scaleY = terr.terrainData.heightmapHeight / ((float)w);
                int layers = terr.terrainData.alphamapLayers;
                float[, ,] alphaDatas = new float[w, h, layers];
                for (int ly = layers - 1; ly >= 0; ly--) {
                    NodeBase tempNode = null;
                    for (int i = 0; i < textureOutputs.Count; i++) {
                        if (textureOutputs[i].paintOrder == ly) {
                            tempNode = textureOutputs[i];
                        }
                    }
                    if (tempNode != null) {
                        float[,] values = tempNode.update(seed, baseX, baseY, w, h, scaleX, scaleY);
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

        public void updateTerrainGrass()
        {
            Terrain terr = Terrain.activeTerrain;
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
                float scaleX = terr.terrainData.heightmapWidth / ((float)w);
                float scaleY = terr.terrainData.heightmapHeight / ((float)w);
                int layers = terr.terrainData.detailPrototypes.Length;
                for (int layer = 0; layer < grassOutputs.Count; layer++) {
                    GrassOutput texNode = grassOutputs[layer];
                    float[,] values = texNode.update(seed, baseX, baseY, w, h, scaleX, scaleY);
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

        private List<TreeInstance> GetTreeInstance(Terrain terrain, bool bEntity)
        {
            int terrainWidth = terrain.terrainData.heightmapWidth;
            int terrainHeight = terrain.terrainData.heightmapHeight;
            int maxLayers = terrain.terrainData.treePrototypes.Length;
            float pixelSize = terrain.terrainData.size.x / terrainWidth;

            List<TreeInstance> instances = new List<TreeInstance>();
            for (int n = 0; n < nodeContainers.Count; n++) {
                if (nodeContainers[n].node is TreeOutput) {
                    TreeOutput treeNode = nodeContainers[n].node as TreeOutput;
                    if (bEntity == treeNode.bEntity) {
                        if (treeNode.treeIndex < maxLayers || bEntity) {
                            float[,] values = treeNode.update(seed, baseX, baseY, terrainWidth, terrainHeight, 1f, 1f);
                            List<Vector3> treePos = new List<Vector3>();
                            List<float> treeAngles = new List<float>();
                            List<float> treeScales = new List<float>();
                            //TreePrototype prot = terr.terrainData.treePrototypes[texNode.treeIndex];
                            for (int x = 0; x < terrainWidth; x++) {
                                for (int y = 0; y < terrainHeight; y++) {
                                    int treeNum = TreeOutput.getTreeNum(x, y, values[x, y], treeNode.density, treeNode.treeIndex + (bEntity ? 29 : 0));
                                    for (int t = 0; t < treeNum; t++) {
                                        Vector2 offset = (TreeOutput.getTreePos(x, y, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
                                        Vector2 pos = new Vector2(y + offset.y, x + offset.x);//翻转x,y.
                                        float height = terrain.terrainData.GetInterpolatedHeight(pos.x / terrainWidth, pos.y / terrainHeight) / terrain.terrainData.size.y;
                                        Vector3 newPos = new Vector3(pos.x / terrainWidth, height, pos.y / terrainHeight);
                                        treePos.Add(newPos);
                                        treeAngles.Add(TreeOutput.GetAngle(x, y, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
                                        treeScales.Add(TreeOutput.GetScale(x, y, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
                                    }
                                }
                            }
                            //
                            for (int i = 0; i < treePos.Count; i++) {
                                TreeInstance ins = new TreeInstance();
                                ins.position = treePos[i];
                                ins.prototypeIndex = treeNode.treeIndex;
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
            }

            return instances;
        }

        public void updateTerrainTree()
        {
            Terrain terr = Terrain.activeTerrain;
            if (terr != null) {
                List<TreePrototype> treeList = new List<TreePrototype>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is TreeOutput) {
                        TreeOutput treeNode = (TreeOutput)(nodeContainers[i].node);
                        if (treeNode.objTree != null && !treeNode.bEntity) {//排除Entity
                            treeNode.treeIndex = treeList.Count;
                            TreePrototype p = new TreePrototype();
                            p.prefab = treeNode.objTree;
                            p.bendFactor = treeNode.bendFactor;
                            treeList.Add(p);
                        }
                    }
                }
                terr.terrainData.treePrototypes = treeList.ToArray();

                List<TreeInstance> instances = GetTreeInstance(terr, false);

                terr.terrainData.treeInstances = instances.ToArray();
            }
        }

        public void updateTerrainEntity(Func<GameObject, GameObject> funNewObj = null)
        {
            Terrain terr = Terrain.activeTerrain;
            if (terr != null) {
                List<TreePrototype> treeList = new List<TreePrototype>();
                for (int i = 0; i < nodeContainers.Count; i++) {
                    if (nodeContainers[i].node is TreeOutput) {
                        TreeOutput treeNode = (TreeOutput)(nodeContainers[i].node);
                        if (treeNode.objTree != null && treeNode.bEntity) {//选取所有Entity
                            treeNode.treeIndex = treeList.Count;
                            TreePrototype p = new TreePrototype();
                            p.prefab = treeNode.objTree;
                            p.bendFactor = treeNode.bendFactor;
                            treeList.Add(p);
                        }
                    }
                }

                GameObject container = GameObject.Find("auto_tree");
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
                    GameObject obj = (funNewObj == null)?GameObject.Instantiate(prefab):funNewObj(prefab);
                    obj.transform.SetParent(container.transform, true);
                    obj.transform.position = Vector3.Scale(instances[i].position, terr.terrainData.size);
                    obj.transform.rotation = prefab.transform.rotation * Quaternion.AngleAxis(instances[i].rotation, Vector3.up);
                    obj.transform.localScale = new Vector3(instances[i].widthScale, instances[i].heightScale, instances[i].widthScale);
                    obj.transform.name = prefab.name;
                }
            }
        }
    }
}
