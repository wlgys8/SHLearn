using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace SHLearn.Editor{

    [CustomEditor(typeof(SHExample))]
    public class SHExampleEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Update")){
                var example = target as SHExample;
                example.Bake();
                SHEditorUtil.UpdateGPUAsyncRequest(example.gpuRequest);
            }
        }
    }
}
