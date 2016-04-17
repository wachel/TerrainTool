using UnityEngine;
using System.Collections;

namespace TerrainTool
{
    public class NodeRefrence : NodeBase
    {
        public string refrenceName;
        public override float[,] update(int seed, int width, int height, Rect rect)
        {
            NodeContainer c = container.container.Find(refrenceName);
            if(c != null && c.node != null){
                return c.node.update(seed, width, height, rect);
            }
            return new float[width, height];
        }
    }
}