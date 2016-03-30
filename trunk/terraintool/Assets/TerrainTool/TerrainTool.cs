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
            container = new NodeContainer();
        }

        public void Start()
        {
            container = new NodeContainer();
            container.node = new NodeConstValue();
        }

        public void OnBeforeSerialize()
        { 
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
