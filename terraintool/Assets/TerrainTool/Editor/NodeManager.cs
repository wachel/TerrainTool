using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;

[Serializable]
public class NodeManager
{
    private static NodeManager g_Manager = null;
    public NodeManager() {
        g_Manager = this;
    }
    public static NodeManager getInstance() {
        return g_Manager;
    }
    public int seed = 0;
    public int baseX = 0;
    public int baseY = 0;
    private Dictionary<string, NodeWindow> nodes = new Dictionary<string, NodeWindow>();
    public List<NodeWindow> nodeList = new List<NodeWindow>();
    public ConfigInfo toConfigInfo() {
        ConfigInfo info = new ConfigInfo();
        info.seed = seed;
        foreach (var node in nodes) {
            info.nodes.Add(node.Value);
        }
        return info;
    }
    private void beforeSave() {
        nodeList.Clear();
        foreach (var n in nodes) {
            n.Value.node.value.beforeSave();
            nodeList.Add(n.Value);
        }
    }
    private void postLoaded() {
        nodes.Clear();
        foreach (var n in nodeList) {
            n.node.value.postLoaded();
            nodes.Add(n.getGuid(), n);
        }
    }
    public static NodeManager createFromConfigInfo(ConfigInfo info) {
        NodeManager rlt = new NodeManager();
        rlt.seed = info.seed;
        foreach (var node in info.nodes) {
            rlt.nodes.Add(node.getGuid(), node);
        }
        return rlt;
    }
    public NodeWindow findNodeWindow(string guid) {
        if (nodes.ContainsKey(guid)) {
            return nodes[guid];
        }
        return null;
    }
    public NodeBase findNode(string guid) {
        if (nodes.ContainsKey(guid)) {
            return nodes[guid].node.value;
        }
        return null;
    }
    public void addNode(NodeWindow node) {
        nodes.Add(node.getGuid(), node);
    }
    public void removeNode(string guid) {
        nodes.Remove(guid);
    }
    public delegate void NodeFun(NodeWindow node);
    public void forEachNodes(NodeFun fun) {
        foreach (var n in nodes) {
            fun(n.Value);
        }
    }
    public delegate int OrderValueFun(NodeWindow node);
    public void forEachNodes_Sorted(OrderValueFun orderFun, NodeFun fun) {
        List<int> orders = new List<int>();
        foreach (var n in nodes) {
            bool bFound = false;
            foreach(var o in orders){
                if(o == orderFun(n.Value)){
                    bFound = true;
                    break;
                }
            }
            if (!bFound) {
                orders.Add(orderFun(n.Value));
            }
        }
        orders.Sort();
        foreach (var o in orders) {
            foreach (var n in nodes) {
                if (orderFun(n.Value) == o) {
                    fun(n.Value);
                }
            }
        }
    }
    public void save(string fileName) {
        beforeSave();
        var serializer = new XmlSerializer(typeof(NodeManager));
        var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
        serializer.Serialize(streamWriter, this);
        stream.Close();
    }
    public static NodeManager load(string fileName) {
        var serializer = new XmlSerializer(typeof(NodeManager));
        var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
        NodeManager info = serializer.Deserialize(streamReader) as NodeManager;
        stream.Close();
        info.postLoaded();
        return info;
    }
}

[Serializable]
public class ConfigInfo
{
    public int seed = 0;
    [System.NonSerialized]
    public List<NodeWindow> nodes = new List<NodeWindow>();
    public void save(string fileName) {
        var serializer = new XmlSerializer(typeof(ConfigInfo));
        var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
        serializer.Serialize(streamWriter, this);
        stream.Close();
    }
    public static ConfigInfo load(string fileName) {
        var serializer = new XmlSerializer(typeof(ConfigInfo));
        var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
        ConfigInfo info = serializer.Deserialize(streamReader) as ConfigInfo;
        stream.Close();
        return info;
    }
}