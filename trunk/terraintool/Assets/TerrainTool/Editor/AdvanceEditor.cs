using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using LibNoise.Unity;
using System;

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

    private NodeWindow startLineNode = null;
    private int startLinePort = 0;

    int curWinID = 100;
    private string curSelectedNode = "";
    public NodeManager nodeManager = null;

    //for tool
    private bool bShowAddTool = true;
    private bool bShowDetailConfig = true;

    //split and resize
    bool bResize = false;
    Rect splitRect;
    float toolWindowWidth = 280;
    Texture2D splitColor;
    Texture2D portColor;

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

        maxRect.Set(0,0,position.width - toolWindowWidth,position.height);


        nodeManager.forEachNodes((n) => {
            n.bNeedUpdate = true;
            n.DrawProperty();
        });        
        Repaint();
    }

    public void setNodeManager(NodeManager nm){
        nodeManager = nm;
        nodeManager.forEachNodes((n) => {
            n.initWindow(getNewWinID(), OnNodeDelete, OnNodeSelected);
        });
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
                nodeManager.forEachNodes((n) => {
                    Rect r = n.Draw();
                    maxRect.width = r.xMax > maxRect.width ? r.xMax : maxRect.width;
                    maxRect.height = r.yMax > maxRect.height ? r.yMax : maxRect.height;
                });
            }
            EndWindows();

            MyDragLine();
            GUI.EndScrollView();
        }
        GUI.EndGroup();

        GUI.BeginGroup(new Rect(splitRect.xMax, 0, toolWindowWidth, position.height),"");
        {
            GUILayout.BeginArea(new Rect(0, 0, toolWindowWidth, position.height));
            ToolWindowOnGUI();
            GUILayout.EndArea();
        }
        GUI.EndGroup();

        MyDragWindow();
        //drawBezier(new Vector2(100, 100), new Vector2(500, 500), Color.white, true) ;
    }

    void MyDragWindow()
    {
        if (Event.current.type == EventType.MouseDrag)
        {
            if (bDragingWindow)
            {
                scrollPosition = startDragMousePos - Event.current.mousePosition + stargDragScrollPos;
                Event.current.Use();
            }
        }
        if (Event.current.type == EventType.MouseDown)
        {
            if (Event.current.button == 2)
            {
                bDragingWindow = true;
                startDragMousePos = Event.current.mousePosition;
                stargDragScrollPos = scrollPosition;
            }
        }
        if (Event.current.type == EventType.MouseUp)
        {
            if (Event.current.button == 2)
            {
                bDragingWindow = false;
            }
        }
    }

    void MyDragLine() {
        if (startLineNode != null) {
            if (bDragingInputLine) {
                Vector2 lineEnd = startLineNode.getInputPortRect(startLinePort).center;
                drawBezier(Event.current.mousePosition, lineEnd, new Color(0.8f, 0.8f, 0.8f), true);
            }
            else if (bDragingOutputLine) {
                Vector2 lineStart = startLineNode.getOutputPortRect().center;
                drawBezier(lineStart,Event.current.mousePosition, new Color(0.8f, 0.8f, 0.8f), true);
            }
        }

        nodeManager.forEachNodes((n) => {
            for (int i = 0; i < n.getInputNum(); i++) {
                var inputNode = nodeManager.findNodeWindow(n.getInputGuid(i));
                if (inputNode != null) {
                    var endPos = n.getInputPortRect(i).center;
                    var startPos = inputNode.getOutputPortRect().center;
                    drawBezier(startPos, endPos, new Color(0.8f, 0.8f, 0.8f), true);
                }
            }
        });

        if (Event.current.type == EventType.MouseDrag) {
            if (bDragingInputLine) {

            }
        }
        if (Event.current.type == EventType.MouseDown) {
            if (Event.current.button == 0) {
                nodeManager.forEachNodes((n) => {
                    for (int i = 0; i < n.getInputNum(); i++) {
                        if (n.getInputPortRect(i).Contains(Event.current.mousePosition)) {
                            if (nodeManager.findNodeWindow(n.getInputGuid(i)) != null) {
                                bDragingOutputLine = true;
                                startLineNode = nodeManager.findNodeWindow(n.getInputGuid(i));
                                n.setInput(i, "");
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
                });
            }
        }
        if (Event.current.type == EventType.MouseUp) {
            if (Event.current.button == 0) {
                if (bDragingInputLine) {
                    nodeManager.forEachNodes((n) => {
                        if (n != startLineNode) {
                            if (n.hasOutput() && n.getOutputPortRect().Contains(Event.current.mousePosition)) {
                                startLineNode.setInput(startLinePort, n.getGuid());
                            }
                        }
                    });
                }
                else if (bDragingOutputLine) {
                    nodeManager.forEachNodes((n) => {
                        if (n != startLineNode) {
                            for (int i = 0; i < n.getInputNum(); i++) {
                                if (n.getInputPortRect(i).Contains(Event.current.mousePosition)) {
                                    n.setInput(i, startLineNode.getGuid());
                                }
                            }
                        }
                    });
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

        if (Event.current.type == EventType.mouseDown && splitRect.Contains(Event.current.mousePosition))
        {
            bResize = true;
        }
        if (bResize)
        {
            toolWindowWidth = position.width - Event.current.mousePosition.x;
            splitRect.Set(Event.current.mousePosition.x - splitRect.width, splitRect.y, splitRect.width, splitRect.height);

            this.Repaint();
        }
        if (Event.current.type == EventType.MouseUp)
        {
            bResize = false;
        }
    }

    void ToolWindowOnGUI()
    {
        GUIStyle myFoldoutStyle = new GUIStyle(EditorStyles.foldout);
        myFoldoutStyle.fontStyle = FontStyle.Bold;
        bShowAddTool = EditorGUILayout.Foldout(bShowAddTool, "Add Node", myFoldoutStyle);
        if (bShowAddTool)
        {
            NodeBase newNode = null;
               GUILayout.Label("Add Generator");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Generator"))
                {
                    newNode = NodeBase.createNewNode(NodeType.Generator);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Add Operator");
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Unary Operator")) {
                    newNode = NodeBase.createNewNode(NodeType.UnaryOperator);
                }
                if (GUILayout.Button("Binary Operator")) {
                    newNode = NodeBase.createNewNode(NodeType.BinaryOperator);
                }
                if (GUILayout.Button("Ternary Operator")) {
                    newNode = NodeBase.createNewNode(NodeType.TernaryOperator);
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.Label("Add Output");
            GUILayout.BeginHorizontal();
            {
                //if (GUILayout.Button("Height Output")) {
                //    newNode = NodeBase.createNewHeightOutput();
                //} 
                if (GUILayout.Button("Texture Output")) {
                    newNode = NodeBase.createNewTextureOutput();
                }
                if (GUILayout.Button("Grass Output")) {
                    newNode = NodeBase.createNewGrassOutput();
                }
                if (GUILayout.Button("Tree Output")) {
                    newNode = NodeBase.createNewTreeOutput();
                }
            }
            GUILayout.EndHorizontal();


            if (newNode != null) {
                Vector2 newPos = this.scrollPosition;
                NodeWindow newNodeWindow = NodeWindow.createNew(newNode, newPos, nodeManager.findNodeWindow, nodeManager.findNode);
                newNodeWindow.initWindow(getNewWinID(), OnNodeDelete, OnNodeSelected);
                newNode.label = getNewName(newNode.label);
                nodeManager.addNode(newNodeWindow);
            }
            GUILayout.Space(20);
        }

        if (nodeManager.findNodeWindow(curSelectedNode) != null) {
            bShowDetailConfig = EditorGUILayout.Foldout(bShowDetailConfig, "Detail Config", myFoldoutStyle);
            if (bShowDetailConfig) {
                var curNode = nodeManager.findNodeWindow(curSelectedNode);
                curNode.DrawProperty();
            }
        }

    }

    void OnNodeDelete(string name)
    {
        nodeManager.removeNode(name);
    }
    void OnNodeSelected(string name) {
        curSelectedNode = name;
    }
    NodeWindow getSelectedNode() {
        return nodeManager.findNodeWindow(curSelectedNode);
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

    private string getNewName(string label) {
        string rlt = label;
        int i = 0;
        while (true) {
            string temp = label + i.ToString();
            bool bFound = false;
            nodeManager.forEachNodes((n) => {
                if ((!bFound) && n.node.value.label == temp) {
                    bFound = true;
                }
            });
            i++;
            if (!bFound) {
                rlt = temp;
                break;
            }
        }
        return rlt;
    }
    private void drawBezier(Vector2 fromPos, Vector2 toPos, Color color, bool bArrow) {
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
