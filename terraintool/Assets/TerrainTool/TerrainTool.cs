using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TerrainTool
{
    public class TerrainTool : MonoBehaviour
    {
        public NodeContainer node = new NodeContainer();
        [HideInInspector]
        public List<NodeBase> nodes = new List<NodeBase>();

    }
}
