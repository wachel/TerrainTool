using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LibNoise.Unity;
using System;

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
        //private string curSelectedNode = "";
        //public NodeManager nodeManager = null;
        private List<NodeContainer> nodeContainers = new List<NodeContainer>();
        private NodeContainer curSelectedNode = null;
        private SerializedObject serializedObject = null;

        //for tool
        private bool bShowAddTool = true;
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
            //nodeManager.forEachNodes((n) => {
            //    n.bNeedUpdate = true;
            //    n.DrawProperty();
            //});
            Repaint();
        }

        //public void setNodeManager(NodeManager nm)
        //{
        //    nodeManager = nm;
        //    nodeManager.forEachNodes((n) => {
        //        n.initWindow(getNewWinID(), OnNodeDelete, OnNodeSelected);
        //    });
        //}

        public void SetNodeContainers(List<NodeContainer> containers)
        {
            this.nodeContainers = containers;
            foreach (var n in nodeContainers) {
                n.winID = getNewWinID();
                //rect.Set(pos.x, pos.y, rect.width, rect.height);
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
                container.bNeedUpdate = true;
            }
            GUI.Label(new Rect(0, 0, container.rect.width, 16), container.node.name);
            if ((!(container.node is HeightOutput)) && GUI.Button(new Rect(container.rect.width - 16, 0, 16, 16), "X")) {
                DeleteNode(container);
            }
            GUI.Box(new Rect(2, 16 + 2, container.rect.width - 4, container.rect.height - 16 - 4), container.tex);
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
            }
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
                    var inputNode = n.inputs[i];
                    if (inputNode != null) {
                        var endPos = n.getInputPortRect(i).center;
                        var startPos = inputNode.getOutputPortRect().center;
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
                                if (n.inputs[i] != null) {
                                    bDragingOutputLine = true;
                                    startLineNode = n.inputs[i];
                                    n.inputs[i] = null;
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
                                    startLineNode.inputs[startLinePort] = n;
                                }
                            }
                        }
                    }
                    else if (bDragingOutputLine) {
                        foreach(var n in nodeContainers){
                            if (n != startLineNode) {
                                for (int i = 0; i < n.getInputNum(); i++) {
                                    if (n.getInputPortRect(i).Contains(Event.current.mousePosition)) {
                                        n.inputs[i] = startLineNode;
                                    }
                                }
                            }
                        }
                    }
                    bDragingInputLine = false;
                    bDragingOutputLine = false;
                    this.Repaint();
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
            bShowAddTool = EditorGUILayout.Foldout(bShowAddTool, "Add Node", myFoldoutStyle);
            if (bShowAddTool) {
                NodeBase newNode = null;
                GUILayout.Label("Add Generator");
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Generator")) {
                        newNode = NodeMaker.Instance.CreateNode(NodeType.Generator);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("Add Operator");
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Unary")) {
                        newNode = NodeMaker.Instance.CreateNode(NodeType.UnaryOperator);
                    }
                    if (GUILayout.Button("Binary")) {
                        newNode = NodeMaker.Instance.CreateNode(NodeType.BinaryOperator);
                    }
                    if (GUILayout.Button("Ternary")) {
                        newNode = NodeMaker.Instance.CreateNode(NodeType.TernaryOperator);
                    }
                }
                GUILayout.EndHorizontal();


                GUILayout.Label("Add Output");
                GUILayout.BeginHorizontal();
                {
                    string[] subTypes = NodeMaker.Instance.GetSubTypes(NodeType.Output);
                    foreach(string t in subTypes){
                        if (GUILayout.Button(t)) {
                            newNode = NodeMaker.Instance.CreateNode(NodeType.Output, t);
                        } 
                    }
                }
                GUILayout.EndHorizontal();

                if (newNode != null) {
                    NodeContainer c = new NodeContainer();
                    c.SetNode(newNode);
                    c.name = "";
                    c.winID = getNewWinID();
                    nodeContainers.Add(c);
                    //Vector2 newPos = this.scrollPosition;
                    //NodeWindow newNodeWindow = NodeWindow.createNew(newNode, newPos, nodeManager.findNodeWindow, nodeManager.findNode);
                    //newNodeWindow.initWindow(getNewWinID(), OnNodeDelete, OnNodeSelected);
                    //newNode.label = getNewName(newNode.label);
                    //nodeManager.addNode(newNodeWindow);
                }
                GUILayout.Space(20);
            }

            if (curSelectedNode != null) {
                bShowDetailConfig = EditorGUILayout.Foldout(bShowDetailConfig, "Detail Config", myFoldoutStyle);
                if (bShowDetailConfig) {
                    DrawNodeProperty(curSelectedNode);
                }
            }
        }

        void DrawNodeProperty(NodeContainer container)
        {
            if (serializedObject != null) {
                PropertyDrawer.DrawProperty(serializedObject);
            }
        }

        void DeleteNode(NodeContainer container)
        {
            nodeContainers.Remove(container);
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
        public void save()
        {

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
            float length = (Mathf.Abs(fromPos.x - toPos.x) + Mathf.Abs(fromPos.y - toPos.y)) * 0.8f + 20f;
            if (fromPos.x - toPos.x > length) {
                pos1 = new Vector3((fromPos.x + toPos.x) * 0.5f, fromPos.y, 0);
                pos2 = new Vector3((fromPos.x + toPos.x) * 0.5f, toPos.y, 0);
            }
            else {
                pos1 = new Vector3(fromPos.x - length * 0.5f, fromPos.y, 0);
                pos2 = new Vector3(toPos.x + length * 0.5f, toPos.y, 0);
            }
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


class PropertyDrawer : Editor
{
    public static void DrawProperty(SerializedObject obj){
        DrawPropertiesExcluding(obj);
    }
}