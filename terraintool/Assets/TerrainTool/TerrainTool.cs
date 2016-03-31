using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TerrainTool
{
    [ExecuteInEditMode]
    public class TerrainTool : MonoBehaviour , ISerializationCallbackReceiver
    {
        public NodeContainer container;
        public void OnEnable()
        {
            container = ScriptableObject.CreateInstance<NodeContainer>();
            container.node = ScriptableObject.CreateInstance <NodeConstValue>();
        }

        public void Start()
        {
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
        }
    }
}
