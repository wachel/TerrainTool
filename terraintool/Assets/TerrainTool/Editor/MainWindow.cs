using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

class MainWindow : EditorWindow
{
    const string assetPath = "Assets/TerrainTool.asset";
    public string path = "";
    public float hillSize = 0.5f;
    private Vector2 scrollviewPosition;
    AdvanceEditor advanceWindow = null;
    NodeManager nodeManager = null;
    [MenuItem("Window/TerrainTool")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(MainWindow));
    }
     
    void init()
    {
        minSize = new Vector2(300, 50);
        nodeManager = new NodeManager();
        NodeBase heightOutput = NodeBase.createNewHeightOutput();
        nodeManager.addNode(NodeWindow.createNew(heightOutput, new Vector2(0f, 100f), nodeManager.findNodeWindow,nodeManager.findNode));
        //AssetDatabase.CreateAsset(this, "assert");
    }

    void OnEnable()
    {
        init();
        NodeManager temp = NodeManager.load("temp.terr_info");
        if (temp != null) {
            nodeManager = temp;
            postNodeMangerLoaded();
        }
        //ConfigInfo info = (ConfigInfo)AssetDatabase.LoadAssetAtPath(p, typeof(ConfigInfo));
        //nodeManager = NodeManager.createFromConfigInfo(info);
        //NodeManager temp = (NodeManager)AssetDatabase.LoadAssetAtPath(assetPath, typeof(NodeManager));
        //if (temp != null) {
        //    nodeManager = temp;
        //}
        //postNodeMangerLoaded();
    }
    void OnDisable() {
        nodeManager.save("temp.terr_info");
        //nodeManager.hideFlags = 0;
        //ConfigInfo info = nodeManager.toConfigInfo();
        //AssetDatabase.CreateAsset(nodeManager, assetPath);
    }
    void OnDestroy() {
        //string p = "Assets/TerrainTool4.asset";
        //nodeManager.hideFlags = 0;
        //ConfigInfo info = nodeManager.toConfigInfo();
        //AssetDatabase.CreateAsset(info, p);
        //AssetDatabase.SaveAssets();
    }
    void OnGUI()
    {
        scrollviewPosition = GUILayout.BeginScrollView(scrollviewPosition);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New")) { DoNew(); }
        if (GUILayout.Button("Load")) { DoLoad(); }
        if (GUILayout.Button("Save")) { DoSave(); }
        GUILayout.Space(100);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        nodeManager.seed = EditorGUILayout.IntField("seed", nodeManager.seed);
        nodeManager.baseX = EditorGUILayout.IntField("offset.x", nodeManager.baseX);
        nodeManager.baseY = EditorGUILayout.IntField("offset.y", nodeManager.baseY);
        NodeManager.OrderValueFun orderFun = (n) => {
            if (n.node.value.getNodeType() == NodeType.HeightOutput) {
                return 0;
            }
            else if (n.node.value.getNodeType() == NodeType.TextureOutput) {
                return ((TextureOutput)n.node.value).paintOrder + 1000;
            }
            else if (n.node.value.getNodeType() == NodeType.GrassOutput) {
                return 2000;
            }
            else if (n.node.value.getNodeType() == NodeType.TreeOutput) {
                return 3000;
            }
            return 0;
        };
        nodeManager.forEachNodes_Sorted(orderFun, (n) => n.node.value.OnMainGUI());

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("       Generate       ")) { DoGenerate(); }
        if (GUILayout.Button("Advance>>")) { DoAdvance(); }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.EndScrollView();

    }
    void DoNew() {
        path = "";
        init();
        postNodeMangerLoaded();
    }

    void DoLoad() {
        path = EditorUtility.OpenFilePanel("Open", "", "terr_info");
        if (path != "") {
            nodeManager = NodeManager.load(path);
            postNodeMangerLoaded();
            //string p = "Assets/TerrainTool2.asset";
            //nodeManager.hideFlags = 0;
            //nodeInfo = nodeManager.toConfigInfo();
            //AssetDatabase.CreateAsset(nodeInfo, p);
            //AssetDatabase.SaveAssets();
            //AssetDatabase.AddObjectToAsset(nodeManager.toConfigInfo(), p);
            //AssetDatabase.CreateAsset(nodeManager, "Assets/TerrainTool.asset");
        }  
    }
    void postNodeMangerLoaded() {
        //nodeManager.postLoaded();
        nodeManager.forEachNodes((n) => n.node.value.setFun(nodeManager.findNode));
        if (advanceWindow) {
            advanceWindow.setNodeManager(nodeManager);
            advanceWindow.init();
        }
    }
    public void beforeSaveScene() {
        //nodeManager.beforeSave();
    }
    void DoSave() {
        //nodeManager.beforeSave();
        //if (path == "") {
            path = EditorUtility.SaveFilePanel("Open", "", "default", "terr_info");
        //}
        if (path != "") {
            nodeManager.save(path);
        }
    }
    void DoGenerate()
    {
        float startTime = Time.realtimeSinceStartup;
        Terrain terr = Terrain.activeTerrain;
        if (terr != null) {
            Undo.RegisterUndo(terr.terrainData, "generate terrain");
            nodeManager.forEachNodes((n) => {
                if (n.node.value.getNodeType() == NodeType.HeightOutput) {
                    updateTerrainHeight(n.node.value);
                }
            });
            updateTerrainTexture();
            updateTerrainGrass();
            updateTerrainTree();
            updateTerrainEntity();
            EditorUtility.SetDirty(terr);
        }
        float diff = Time.realtimeSinceStartup - startTime;
        Debug.Log("time = " + diff.ToString());  
    }
    private void updateTerrainHeight(NodeBase node){
        Terrain terr = Terrain.activeTerrain;
        if(terr != null){
            int w = terr.terrainData.heightmapWidth;
            int h = terr.terrainData.heightmapHeight;
            //int cx = (w-1) / 128 + 1;
            //int cy = (h-1) / 128 + 1;
            //for (int x = 0; x < cx; x++) {
            //    for (int y = 0; y < cy; y++) {
            //        int sw = (x + 1) * 128 < w ? 128 : (w - x * 128);
            //        int sh = (y + 1) * 128 < h ? 128 : (h - y * 128);
            //        float[,] values = node.update(nodeManager.seed, nodeManager.baseX + x * 128, nodeManager.baseY + y * 128, sw, sh);
            //        terr.terrainData.SetHeights(y * 128,x * 128,  values);
            //    }
            //}
            float[,] values = node.update(nodeManager.seed, nodeManager.baseX, nodeManager.baseY, w, h);
            terr.terrainData.SetHeights(0, 0, values);
        }
    }
    private void updateTerrainTexture() {
        Terrain terr = Terrain.activeTerrain;
        if (terr != null) {
            List<SplatPrototype> prototypes = new List<SplatPrototype>();
            NodeManager.OrderValueFun orderFun = (n)=>{
                if (n.node.value.getNodeType() == NodeType.TextureOutput) {
                    return ((TextureOutput)n.node.value).paintOrder;
                }
                return 0;
            };
            nodeManager.forEachNodes_Sorted(orderFun, (n) => {
                if (n.node.value.getNodeType() == NodeType.TextureOutput) {
                    TextureOutput texNode = (TextureOutput)(n.node.value);
                    if (texNode.Texture != null) {
                        texNode.textureIndex = prototypes.Count;
                        SplatPrototype p = new SplatPrototype();
                        p.texture = texNode.Texture;
                        p.normalMap = texNode.Normal;
                        p.tileSize = new Vector2(texNode.texSizeX, texNode.texSizeY);
                        prototypes.Add(p);
                    }
                }
            });
            terr.terrainData.splatPrototypes = prototypes.ToArray();
            int w = terr.terrainData.alphamapWidth;
            int h = terr.terrainData.alphamapHeight;
            float[,] totalAlpha = new float[w, h];
            float scaleX = terr.terrainData.heightmapWidth / ((float)w);
            float scaleY = terr.terrainData.heightmapHeight / ((float)w);
            int layers = terr.terrainData.alphamapLayers;
            float[, ,] alphaDatas = new float[w,h,layers];//terr.terrainData.GetAlphamaps(0, 0, w, h);
            for (int l = layers - 1; l >= 0; l--) {
                NodeBase tempNode = null;
                nodeManager.forEachNodes((n) => {
                    if (n.node.value.getNodeType() == NodeType.TextureOutput) {
                        if (((TextureOutput)n.node.value).paintOrder == l) {
                            tempNode = n.node.value;
                        }
                    }
                });
                if (tempNode != null) {
                    float[,] values = tempNode.update(nodeManager.seed, nodeManager.baseX, nodeManager.baseY, w, h, scaleX, scaleY);
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            if (totalAlpha[i, j] + values[i, j] <= 1.00001f) {
                                alphaDatas[i, j, l] = values[i, j];
                                totalAlpha[i, j] += values[i, j];
                            }
                            else {
                                alphaDatas[i, j, l] = 1f - totalAlpha[i, j];
                                totalAlpha[i, j] = 1f;
                            }
                        }
                    }
                }
            }
            terr.terrainData.SetAlphamaps(0, 0, alphaDatas);
        }
    }
    private void updateTerrainGrass() {
        Terrain terr = Terrain.activeTerrain;
        if (terr != null) {
            List<DetailPrototype> detailList = new List<DetailPrototype>();
            nodeManager.forEachNodes((n) => {
                if (n.node.value.getNodeType() == NodeType.GrassOutput) {
                    GrassOutput texNode = (GrassOutput)(n.node.value);
                    if (texNode.Texture != null) {
                        texNode.grassIndex = detailList.Count;
                        DetailPrototype p = new DetailPrototype();
                        p.prototypeTexture = texNode.Texture;
                        p.renderMode = texNode.bBillboard ? DetailRenderMode.GrassBillboard : DetailRenderMode.Grass;
                        detailList.Add(p);
                    }
                }
            });
            terr.terrainData.detailPrototypes = detailList.ToArray();
            int w = terr.terrainData.detailWidth;
            int h = terr.terrainData.detailHeight;
            float scaleX = terr.terrainData.heightmapWidth / ((float)w);
            float scaleY = terr.terrainData.heightmapHeight / ((float)w);
            int layers = terr.terrainData.detailPrototypes.Length;
            nodeManager.forEachNodes((n) => {
                if (n.node.value.getNodeType() == NodeType.GrassOutput) {
                    GrassOutput texNode = (GrassOutput)n.node.value;
                    float[,] values = texNode.update(nodeManager.seed, nodeManager.baseX, nodeManager.baseY, w, h, scaleX, scaleY);
                    int[,] detailValues = new int[w, h];
                    for (int i = 0; i < w; i++) {
                        for (int j = 0; j < h; j++) {
                            detailValues[i, j] = (int)(values[i, j] * values[i, j] * 16);
                        }
                    }
                    if (texNode.grassIndex < layers) {
                        terr.terrainData.SetDetailLayer(0, 0, texNode.grassIndex, detailValues);
                    }
                }
            });
        }
    }

    private List<TreeInstance> GetTreeInstance(Terrain terrain,bool bEntity)
    {
        int terrainWidth = terrain.terrainData.heightmapWidth;
        int terrainHeight = terrain.terrainData.heightmapHeight;
        int maxLayers = terrain.terrainData.treePrototypes.Length;
        float pixelSize = terrain.terrainData.size.x / terrainWidth;

        List<TreeInstance> instances = new List<TreeInstance>();
        nodeManager.forEachNodes((n) => {
            if (n.node.value.getNodeType() == NodeType.TreeOutput) {
                TreeOutput treeNode = (TreeOutput)n.node.value;
                if (bEntity == treeNode.bEntity) {
                    if (treeNode.treeIndex < maxLayers || bEntity) {
                        float[,] values = treeNode.update(nodeManager.seed, nodeManager.baseX, nodeManager.baseY, terrainWidth, terrainHeight, 1f, 1f);
                        List<Vector3> treePos = new List<Vector3>();
                        List<float> treeAngles = new List<float>();
                        List<float> treeScales = new List<float>();
                        //TreePrototype prot = terr.terrainData.treePrototypes[texNode.treeIndex];
                        for (int i = 0; i < terrainWidth; i++) {
                            for (int j = 0; j < terrainHeight; j++) {
                                int treeNum = TreeOutput.getTreeNum(i, j, values[i, j], treeNode.density, treeNode.treeIndex + (bEntity ? 29 : 0));
                                for (int t = 0; t < treeNum; t++) {
                                    Vector2 offset = (TreeOutput.getTreePos(i, j, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
                                    Vector2 pos = new Vector2(j + offset.y, i + offset.x);//翻转x,y.
                                    float height = terrain.terrainData.GetInterpolatedHeight(pos.x / terrainWidth, pos.y / terrainHeight) / terrain.terrainData.size.y;
                                    Vector3 newPos = new Vector3(pos.x / terrainWidth, height, pos.y / terrainHeight);
                                    treePos.Add(newPos);
                                    treeAngles.Add(TreeOutput.GetAngle(i, j, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
                                    treeScales.Add(TreeOutput.GetScale(i, j, t, pixelSize * 2f, treeNode.treeIndex + (bEntity ? 29 : 0)));
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
                            float s = Mathf.Lerp(treeNode.minSize, treeNode.maxSize,treeScales[i]);
                            ins.heightScale = s;
                            ins.widthScale = s;
                            ins.rotation = 360 * treeAngles[i];
                            instances.Add(ins);
                        }
                    }
                }
            }
        });

        return instances;
    }

    private void updateTerrainTree() {
        Terrain terr = Terrain.activeTerrain;
        if (terr != null) {
            List<TreePrototype> treeList = new List<TreePrototype>();
            nodeManager.forEachNodes((n) => {
                if (n.node.value.getNodeType() == NodeType.TreeOutput) {
                    TreeOutput treeNode = (TreeOutput)(n.node.value);
                    if (treeNode.ObjTree != null && !treeNode.bEntity) {
                        treeNode.treeIndex = treeList.Count;
                        TreePrototype p = new TreePrototype();
                        p.prefab = treeNode.ObjTree;
                        p.bendFactor = treeNode.bendFactor;
                        treeList.Add(p);
                    }
                }
            });
            terr.terrainData.treePrototypes = treeList.ToArray();

            List<TreeInstance> instances = GetTreeInstance(terr,false);

            terr.terrainData.treeInstances = instances.ToArray();
        }
    }

    private void updateTerrainEntity()
    {
        Terrain terr = Terrain.activeTerrain;
        if (terr != null) {
            List<TreePrototype> treeList = new List<TreePrototype>();
            nodeManager.forEachNodes((n) => {
                if (n.node.value.getNodeType() == NodeType.TreeOutput) {
                    TreeOutput treeNode = (TreeOutput)(n.node.value);
                    if (treeNode.ObjTree != null && treeNode.bEntity) {
                        treeNode.treeIndex = treeList.Count;
                        TreePrototype p = new TreePrototype();
                        p.prefab = treeNode.ObjTree;
                        p.bendFactor = treeNode.bendFactor;
                        treeList.Add(p);
                    }
                }
            });

            GameObject container = GameObject.Find("auto_tree");
            if (container == null) {
                container = new GameObject("auto_tree");
                EditorUtility.SetDirty(container);
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
            
            for(int i = 0; i<instances.Count; i++) {
                GameObject prefab = treeList[instances[i].prototypeIndex].prefab;
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                //GameObject obj = (GameObject)GameObject.Instantiate(prefab);
                
                obj.transform.SetParent(container.transform, true);
                obj.transform.position = Vector3.Scale(instances[i].position , terr.terrainData.size);
                obj.transform.rotation = prefab.transform.rotation * Quaternion.AngleAxis(instances[i].rotation,Vector3.up);
                obj.transform.localScale = new Vector3(instances[i].widthScale, instances[i].heightScale, instances[i].widthScale);
                obj.transform.name = prefab.name;
                //obj.isStatic = true;
            }
            //{
            //    Transform[] childs = container.GetComponentsInChildren<Transform>();
            //    foreach (Transform child in childs) {
            //        if (child != container.transform) {
            //            if (child != null) {
            //                child.gameObject.isStatic = true;
            //            }
            //        }
            //    }
            //}

        }
    }

    void test()
    {
        TreeInstance[] instances = new TreeInstance[100];//种100棵树
        for (int i = 0; i < 100; i++)
        {
            TreeInstance ins = new TreeInstance();
            ins.position = new Vector3(Random.Range(0f,100.0f),0,Random.Range(0f,100.0f));//随机位置
            ins.prototypeIndex = i % 3;//对应第几种树
            ins.color = Color.white;
            ins.lightmapColor = Color.white;
            float s = Random.Range(0.8f, 1.2f);
            ins.heightScale = s;
            ins.widthScale = s;
            instances[i] = ins;
        }
        Terrain.activeTerrain.terrainData.treeInstances = instances;
    }
    void DoAdvance()
    {
        advanceWindow = (AdvanceEditor)EditorWindow.GetWindow(typeof(AdvanceEditor));
        advanceWindow.setNodeManager(nodeManager);
        advanceWindow.init();
    }
}

public class FileModificationWarning : UnityEditor.AssetModificationProcessor
{
    static string[] OnWillSaveAssets(string[] paths) {
        NodeManager.getInstance().save("temp.terr_info");
        return paths;
    }
}