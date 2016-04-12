using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LibNoise.Unity;
using System;
using UnityEditor.SceneManagement;

namespace TerrainTool
{
    class AdvanceEditor : EditorWindow
    {
        //for drag window
        private Vector2 scrollPosition;
        private bool bDragingWindow = false;
        private bool bDragingInputLine = false;
        private bool bDragingOutputLine = false;
        private Vector2 startDragMousePos;
        private Vector2 stargDragScrollPos;
        Rect maxRect;

        //for drag line
        private NodeContainer startLineNode = null;
        private int startLinePort = 0;

        //
        int curWinID = 100;
        private TerrainTool target;
        private List<NodeContainer> nodeContainers = new List<NodeContainer>();
        private NodeContainer curSelectedNode = null;
        private SerializedObject serializedObject = null;

        //for tool
        private bool bShowDetailConfig = true;

        //split and resize
        bool bResize = false;
        Rect splitRect;
        float toolWindowWidth = 280;
        Texture2D splitColor;
        Texture2D portColor;
        Texture2D texPort;


        public void init()
        {
            minSize = new Vector2(400, 400);
            splitColor = new Texture2D(1, 1);
            splitColor.SetPixel(0, 0, Color.gray);
            splitColor.Apply();

            portColor = new Texture2D(1, 1);
            portColor.SetPixel(0, 0, Color.white);
            portColor.Apply();

            splitRect.Set(position.width - toolWindowWidth, 0, 5, position.height);

            maxRect.Set(0, 0, position.width - toolWindowWidth, position.height);


            texPort = new Texture2D(1, 1);
            texPort.SetPixel(0, 0, new Color(1, 1, 1));
            texPort.Apply();
            Repaint();
        }

        public void SetTarget(TerrainTool target)
        {
            this.target = target;
            this.nodeContainers = target.nodeContainers;
            foreach (var n in nodeContainers) {
                n.winID = getNewWinID();
            }
            SetNodeSelected(null);
        }

        void OnEnable()
        {
            //init();
        }

        void OnGUI()
        {
            splitRect.Set(position.width - toolWindowWidth - splitRect.width, 0, splitRect.width, position.height);
            ResizeSplit();
            GUI.BeginGroup(new Rect(0, 0, splitRect.x, position.height));
            {
                GUI.DrawTexture(new Rect(0, 0, splitRect.x, position.height), splitColor);
                scrollPosition = GUI.BeginScrollView(new Rect(0, 0, splitRect.x, position.height), scrollPosition, maxRect, true, true);

                maxRect.Set(0, 0, splitRect.x, position.height);
                BeginWindows();
                {
                    foreach (var n in nodeContainers) {
                        Rect r = DrawNodeWindow(n);
                        maxRect.width = r.xMax > maxRect.width ? r.xMax : maxRect.width;
                        maxRect.height = r.yMax > maxRect.height ? r.yMax : maxRect.height;
                        if(r != n.rect) {
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
                EndWindows();

                MyDragLine();
                GUI.EndScrollView();
            }
            GUI.EndGroup();

            GUI.BeginGroup(new Rect(splitRect.xMax, 0, toolWindowWidth, position.height), "");
            {
                GUILayout.BeginArea(new Rect(0, 0, toolWindowWidth, position.height));
                ToolWindowOnGUI();
                GUILayout.EndArea();
            }
            GUI.EndGroup();

            MyDragWindow();
            //drawBezier(new Vector2(100, 100), new Vector2(500, 500), Color.white, true) ;
        }

        private Rect DrawNodeWindow(NodeContainer container)
        {
            container.rect.x = container.rect.x < 0 ? 0 : container.rect.x;
            container.rect.y = container.rect.y < 0 ? 0 : container.rect.y;
            container.rect = GUI.Window(container.winID, container.rect, (int id) => { DoNodeWindowGUI(container); }, "");

            if (container.hasOutput()) {
                GUI.DrawTexture(new Rect(container.rect.x - 14, container.rect.center.y - 9, 14, 18), texPort);
                GUI.Label(new Rect(container.rect.x - 14, container.rect.center.y - 9, 14, 18), "o");
            }
            for (int i = 0; i < container.getInputNum(); i++) {
                GUI.DrawTexture(container.getInputPortRect(i), texPort);
                GUI.Label(container.getInputPortRect(i), container.GetInputLabel(i));
            }

            return container.rect;
        }

        private void DoNodeWindowGUI(NodeContainer container)
        {
            if (Event.current.type == EventType.MouseDrag) {
            }
            else if (Event.current.type == EventType.MouseDown) {
                SetNodeSelected(container);
                container.oldHashCode = 0;
            }
            GUI.Label(new Rect(0, 0, container.rect.width, 16), container.node.name);
            if (GUI.Button(new Rect(container.rect.width - 16, 0, 16, 16), "X")) {
                DeleteNode(container);
            }
            int newHashCode = container.GetMyHashCode();
            if (container.oldHashCode != newHashCode) {
                container.updatePreviewTexture(target.seed, 0, 0, 128, 128);
                container.oldHashCode = newHashCode;
            }
            GUI.Box(new Rect(0, 16, container.rect.width, container.rect.height - 16), container.texture);

            //显示公式
            GUIStyle greenStyle = new GUIStyle(); greenStyle.normal.textColor = Color.green;
            EditorGUILayout.LabelField(container.node.GetExpression(), greenStyle);
            if(container.node is NodeCurve) {
                AnimationCurve curve = (container.node as NodeCurve).curve;
                Vector3[] points = new Vector3[41];
                for (int i = 0; i < 41; i++) {
                    float x0 = i / 40.0f;
                    float y0 = curve.Evaluate(x0);
                    points[i].Set(x0 * 124+2, 128 - y0 * 124 - 2 + 16, 0);
                }
                Handles.color = Color.green;
                Handles.DrawAAPolyLine(points);
            }

            //拖动窗口
            GUI.DragWindow();
        }

        void MyDragWindow()
        {
            if (Event.current.type == EventType.MouseDrag) {
                if (bDragingWindow) {
                    scrollPosition = startDragMousePos - Event.current.mousePosition + stargDragScrollPos;
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.MouseDown) {
                if (Event.current.button == 2) {
                    bDragingWindow = true;
                    startDragMousePos = Event.current.mousePosition;
                    stargDragScrollPos = scrollPosition;
                }
            }
            if (Event.current.type == EventType.MouseUp) {
                if (Event.current.button == 2) {
                    bDragingWindow = false;
                }
                if(Event.current.button == 1) {
                    ShowPopupMenu_OnBkg();
                }
            }
        }

        void ShowPopupMenu_OnBkg()
        {
            GenericMenu toolsMenu = new GenericMenu();
            string[] types = Enum.GetNames(typeof(NodeType));
            for (int i = 0; i < types.Length; i++) {
                NodeType nodeType = (NodeType)i;
                string[] subTypes = NodeMaker.Instance.GetSubTypes(nodeType);
                foreach (string st in subTypes) {
                    string sub = st;
                    Vector2 pos = Event.current.mousePosition;
                    toolsMenu.AddItem(new GUIContent("Add Node/" + types[i] + "/" + st), false, () => {
                        AddNode(NodeMaker.Instance.CreateNode(nodeType, sub), pos);
                    });
                }
            }
            toolsMenu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));
        }

        void MyDragLine()
        {
            if (startLineNode != null) {
                if (bDragingInputLine) {
                    Vector2 lineEnd = startLineNode.getInputPortRect(startLinePort).center;
                    drawBezier(Event.current.mousePosition, lineEnd, new Color(0.8f, 0.8f, 0.8f), true);
                }
                else if (bDragingOutputLine) {
                    Vector2 lineStart = startLineNode.getOutputPortRect().center;
                    drawBezier(lineStart, Event.current.mousePosition, new Color(0.8f, 0.8f, 0.8f), true);
                }
            }

            foreach(var n in nodeContainers){
                for (int i = 0; i < n.getInputNum(); i++) {
                    var inputNode = n.node.inputs[i];
                    if (inputNode != null) {
                        var endPos = n.getInputPortRect(i).center;
                        var startPos = inputNode.container.getOutputPortRect().center;
                        drawBezier(startPos, endPos, new Color(0.8f, 0.8f, 0.8f), true);
                    }
                }
            }

            if (Event.current.type == EventType.MouseDrag) {
                if (bDragingInputLine) {

                }
            }
            if (Event.current.type == EventType.MouseDown) {
                if (Event.current.button == 0) {
                    foreach(var n in nodeContainers){
                        for (int i = 0; i < n.getInputNum(); i++) {
                            if (n.getInputPortRect(i).Contains(Event.current.mousePosition)) {
                                if (n.node.inputs[i] != null) {
                                    bDragingOutputLine = true;
                                    startLineNode = n.node.inputs[i].container;
                                    n.node.inputs[i] = null;
                                }
                                else {
                                    bDragingInputLine = true;
                                    startLineNode = n;
                                    startLinePort = i;
                                }
                            }
                        }
                        if (n.hasOutput()) {
                            if (n.getOutputPortRect().Contains(Event.current.mousePosition)) {
                                bDragingOutputLine = true;
                                startLineNode = n;
                            }
                        }
                    }
                }
            }
            if (Event.current.type == EventType.MouseUp) {
                if (Event.current.button == 0) {
                    if (bDragingInputLine) {
                        foreach(var n in nodeContainers){
                            if (n != startLineNode) {
                                if (n.hasOutput() && n.getOutputPortRect().Contains(Event.current.mousePosition)) {
                                    startLineNode.node.inputs[startLinePort] = n.node;
                                }
                            }
                        }
                    }
                    else if (bDragingOutputLine) {
                        foreach(var n in nodeContainers){
                            if (n != startLineNode) {
                                for (int i = 0; i < n.getInputNum(); i++) {
                                    if (n.getInputPortRect(i).Contains(Event.current.mousePosition)) {
                                        n.node.inputs[i] = startLineNode.node;
                                    }
                                }
                            }
                        }
                    }
                    bDragingInputLine = false;
                    bDragingOutputLine = false;
                    this.Repaint();

                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }

        private int getNewWinID()
        {
            curWinID++;
            return curWinID;
        }

        private void ResizeSplit()
        {
            //GUI.DrawTexture(splitRect, splitColor);
            EditorGUIUtility.AddCursorRect(splitRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.mouseDown && splitRect.Contains(Event.current.mousePosition)) {
                bResize = true;
            }
            if (bResize) {
                toolWindowWidth = position.width - Event.current.mousePosition.x;
                splitRect.Set(Event.current.mousePosition.x - splitRect.width, splitRect.y, splitRect.width, splitRect.height);

                this.Repaint();
            }
            if (Event.current.type == EventType.MouseUp) {
                bResize = false;
            }
        }

        void ToolWindowOnGUI()
        {
            GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
            myFoldoutStyle.fontStyle = FontStyle.Bold;

            if (curSelectedNode != null) {
                bShowDetailConfig = EditorGUILayout.Foldout(bShowDetailConfig, "Detail Config", myFoldoutStyle);
                if (bShowDetailConfig) {
                    DrawNodeProperty(curSelectedNode);
                }
            }
        }

        void AddNode(NodeBase node,Vector2 pos)
        {
            NodeContainer c = ScriptableObject.CreateInstance<NodeContainer>();
            c.SetNode(node);
            c.winID = getNewWinID();
            c.rect = new Rect(pos, new Vector2(128, 128 + 16));
            nodeContainers.Add(c);

            target.Sort();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        void DrawNodeProperty(NodeContainer container)
        {
            NodeBase node = container.node;
            if (serializedObject != null) {
                string[] subTypes = NodeMaker.Instance.GetSubTypes(node.nodeType);
                int curIndex = 0;
                for (int i = 0; i < subTypes.Length; i++) {
                    if (subTypes[i] == node.subType) {
                        curIndex = i;
                    }
                }
                int newIndex = EditorGUILayout.Popup("Node Type", curIndex, subTypes);
                if(newIndex != curIndex) {
                    NodeBase newNode = NodeMaker.Instance.CreateNode(node.nodeType, subTypes[newIndex]);
                    newNode.name = node.name;
                    for(int i = 0; i<node.inputs.Length; i++) {
                        newNode.inputs[i] = node.inputs[i];
                    }
                    foreach(var n in nodeContainers) {
                        for(int i =0; i<n.node.inputs.Length; i++) {
                            if(n.node.inputs[i] == node) {
                                n.node.inputs[i] = newNode;
                            }
                        }
                    }
                    container.SetNode(newNode);
                }
                if (SerializedObjectDrawer.DrawProperty(serializedObject)) {
                    //Repaint();
                }
            }
        }

        void DeleteNode(NodeContainer container)
        {
            nodeContainers.Remove(container);
            foreach(var n in nodeContainers) {
                for(int i = 0; i<n.node.inputs.Length; i++) {
                    if(n.node.inputs[i] != null && n.node.inputs[i].container == container) {
                        n.node.inputs[i] = null;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        void SetNodeSelected(NodeContainer container)
        {
            curSelectedNode = container;
            if (container != null) {
                serializedObject = new SerializedObject(container.node);
            }
        }

        void Update()
        {
            if (bDragingInputLine || bDragingOutputLine) {
                this.Repaint();
            }
        }

        private string getNewName(string label)
        {
            string rlt = label;
            int i = 0;
            while (true) {
                string temp = label + i.ToString();
                bool bFound = false;
                foreach(var n in nodeContainers){
                    if (n.name == temp) {
                        bFound = true;
                        break;
                    }
                }
                i++;
                if (!bFound) {
                    rlt = temp;
                    break;
                }
            }
            return rlt;
        }
        private void drawBezier(Vector2 fromPos, Vector2 toPos, Color color, bool bArrow)
        {
            Vector3 pos0 = new Vector3(fromPos.x, fromPos.y, 0);
            Vector3 pos1;
            Vector3 pos2;
            float length = (Mathf.Abs(fromPos.x - toPos.x) + Mathf.Abs(fromPos.y - toPos.y)) * 0.0f + 60f;
            if (fromPos.x - toPos.x > length) {
                pos1 = new Vector3((fromPos.x + toPos.x) * 0.5f, fromPos.y, 0);
                pos2 = new Vector3((fromPos.x + toPos.x) * 0.5f, toPos.y, 0);
            }
            else {
                pos1 = new Vector3(fromPos.x - length * 0.5f, fromPos.y, 0);
                pos2 = new Vector3(toPos.x + length * 0.5f, toPos.y, 0);
            }
            //pos1 = new Vector3(fromPos.x - 50, fromPos.y, 0);
            //pos2 = new Vector3(toPos.x + 50, toPos.y, 0);
            Vector3 pos3 = new Vector3(toPos.x, toPos.y, 0);
            Handles.DrawBezier(pos0, pos3, pos1, pos2, color, null, 3f);
            if (bArrow) {
                Vector3 arrowSideUp = new Vector3(toPos.x + 12, toPos.y - 6, 0);
                Vector3 arrowSideDown = new Vector3(toPos.x + 12, toPos.y + 6, 0);
                Handles.color = color;
                Handles.DrawLine(toPos, arrowSideUp);
                Handles.DrawLine(toPos, arrowSideDown);
            }
        }
    }
}


class SerializedObjectDrawer : Editor
{
    public static bool DrawProperty(SerializedObject so){
        DrawPropertiesExcluding(so, "m_Script");
        if (GUI.changed) {
            so.ApplyModifiedProperties();
            return true;
        }
        return false;
    }
}