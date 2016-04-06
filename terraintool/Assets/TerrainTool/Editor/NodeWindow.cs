using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[Serializable]
public class NodeWindow : Editor
{
    public delegate void DeleteFunc(string guid);
    public delegate void SelectFunc(string guid);
    public delegate NodeWindow FindNodeWindowFun(string guid);
    public Vector2 pos;
    public NodeWrapper node;

    Int32 oldHash = 0;

    private Rect rect;
    private int winID;
    private Texture2D tex;
    private Texture2D texPort;
    private DeleteFunc funDelete;
    private SelectFunc funSelect;
    //private FindNodeWindowFun funFindNodeWindow;
    //private NodeBase.FindNodeFun funFindNode;
    public bool bNeedUpdate = false;

    private GeneratorType generateType;
    private UnaryOperatorType unaryOperatorType;
    private BinaryOperatorType binaryOperatorType;
    //private TernaryOperatorType ternaryOperatorType;

    public int getInputNum() {
        return node.value.getInputNum();
    }
    public Rect getInputPortRect(int index) {
        const int portSize = 16;
        
        float a = 0.1f + (index + 1) * ((1f-0.2f) / (getInputNum() + 1f));
        return new Rect(rect.xMax, rect.yMin + a * rect.height - portSize/2, 14, 16);
    }
    public string getInputGuid(int index) {
        return node.value.getInputGuid(index);
    }
    public bool hasOutput() {
        return node.value.hasOutput();
    }
    public Rect getOutputPortRect() {
        return new Rect(rect.x - 14, rect.center.y - 8, 14, 16);
    }
    public void setInput(int index,string guid) {
        node.value.setInputGuid(index, guid);
    }
    public string getGuid() {
        return node.value.guid;
    }

    public static NodeWindow createNew(NodeBase node, Vector2 pos,FindNodeWindowFun funFind,NodeBase.FindNodeFun funFindNode) {
        NodeWindow rlt = new NodeWindow();
        rlt.node = new NodeWrapper(node);
        rlt.pos = pos;
        //rlt.funFindNodeWindow = funFind;
        rlt.node.value.setFun(funFindNode);
        //rlt.funFindNode = funFindNode;
        return rlt;
    }

    public void setNode(NodeBase n) {
        node = new NodeWrapper(n);
    }

    public void initWindow(int id, DeleteFunc funDel,SelectFunc funSel) {
        winID = id;
        funDelete = funDel;
        funSelect = funSel;
        rect = new Rect(0, 0, 128, 128 + 16);
        rect.Set(pos.x, pos.y, rect.width, rect.height);
        tex = new Texture2D(1,1);
        tex.SetPixel(0, 0, new Color(1,1,1));
        generateType = node.value.getGeneratorType();
        unaryOperatorType = node.value.getUnaryOperatorType();
        binaryOperatorType = node.value.getBinaryOperatorType();
        //ternaryOperatorType = node.value.getTernaryOperatorType();
        texPort = new Texture2D(1, 1);
        texPort.SetPixel(0, 0, new Color(1, 1, 1));
        texPort.Apply();
    }
    
    public Rect Draw()
    {
        rect.x = rect.x < 0 ? 0 : rect.x;
        rect.y = rect.y < 0 ? 0 : rect.y;
        rect = GUI.Window(winID, rect, DoGUI, "");
        pos = rect.min;
        
        if (hasOutput()) {
            GUI.DrawTexture(new Rect(rect.x - 14, rect.center.y - 9, 14, 18), texPort);
            GUI.Label(new Rect(rect.x - 14, rect.center.y - 9, 14, 18), "o");
        }
        for (int i = 0; i < getInputNum(); i++) {
            GUI.DrawTexture(getInputPortRect(i), texPort);
            GUI.Label(getInputPortRect(i), node.value.getInputName(i));
        }
        
        return rect;
    }
    public void DrawProperty() {
        NodeBase newNode = null;
        if (node.value.getNodeType() == NodeType.Generator) {
            GeneratorType newType = (GeneratorType)EditorGUILayout.EnumPopup("Generator Type", generateType);
            if (newType != generateType) {
                newNode = NodeBase.createNewGenerate(newType);
            }
            generateType = newType;
        }
        else if (node.value.getNodeType() == NodeType.UnaryOperator) {
            UnaryOperatorType newType = (UnaryOperatorType)EditorGUILayout.EnumPopup("Operator Type", unaryOperatorType);
            if (newType != unaryOperatorType) {
                newNode = NodeBase.createNewUnaryOperator(newType);
            }
            //    string oldGuid = node.value.guid;
            //    string oldName = node.value.label;
            //    node = new NodeWrapper(NodeBase.createNewUnaryOperator(newType));
            //    node.value.label = oldName;
            //    node.value.guid = oldGuid;
            //}
            unaryOperatorType = newType;
        }
        else if (node.value.getNodeType() == NodeType.BinaryOperator) {
            BinaryOperatorType newType = (BinaryOperatorType)EditorGUILayout.EnumPopup("Operator Type", binaryOperatorType);
            if (newType != binaryOperatorType) {
                ((NodeBinaryOperator)node.value).operatorType = newType;
            }
            binaryOperatorType = newType;
        }
        if (newNode != null) {
            newNode.copy(node.value);
            node = new NodeWrapper(newNode);
        }
        node.value.OnGUI();

        //DrawPropertiesExcluding(new SerializedObject(node.value), "m_Script","inputs","guid");

        int newHashCode = node.value.GetHashCode();
        if (newHashCode != oldHash || bNeedUpdate) {
            bNeedUpdate = false;
            float[,] cls = node.value.update(0, 0, 0, 128, 128);
            //node.value.getPreview();
            int w = 128;
            tex = new Texture2D(w, w);
            Color[] colors = new Color[w * w];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < w; j++) {
                    colors[j * w+i].a = 1;
                    colors[j * w + i].r = cls[i, j];
                    colors[j * w + i].g = cls[i, j];
                    colors[j * w + i].b = cls[i, j];
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
        }
        oldHash = newHashCode;
    }

    void DoGUI(int id)
    {
        if (Event.current.type == EventType.MouseDrag){
        }
        else if (Event.current.type == EventType.MouseDown) {
            funSelect(node.value.guid);
            bNeedUpdate = true;
        }
        GUI.Label(new Rect(0, 0, rect.width, 16),node.value.label);
        if (node.value.getNodeType() != NodeType.HeightOutput && GUI.Button(new Rect(rect.width - 16, 0, 16, 16), "X")){
            funDelete(node.value.guid);
        }
        GUI.Box(new Rect(2, 16+2, rect.width-4, rect.height - 16-4), tex);
        GUI.DragWindow();

        node.value.OnWindowGUI();
    }
}

