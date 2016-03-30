using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TerrainTool
{
    public class TerrainTool : MonoBehaviour
    {
        public NodeBase node = new NodeBase();

        [HideInInspector]
        public List<NodeBase> nodes = new List<NodeBase>();

    }
}
