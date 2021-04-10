using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace SHLearn.Editor{
    public static class SHEditorUtil{


        [MenuItem("Assets/SHLearn/CreateSH9AssetFromCubeMap")]
        public static void CreateSH9AssetFromSelectedCubeMapAsync(){
            Cubemap selectedCube = null;
            foreach(var o in Selection.objects){
                if(o is Cubemap c){
                    selectedCube = c;
                    break;
                }
            }
            if(!selectedCube){
                return;
            }
            CreateSH9AssetFromCubeMapAsync(selectedCube);
        }
    

        public static void CreateSH9AssetFromCubeMapAsync(Cubemap cubemap){
            var proj = new SH9Project();
            var req = proj.FromCubeMapAsync(cubemap,(shc)=>{
                var cubePath = AssetDatabase.GetAssetPath(cubemap);
                var shcAssetPath = cubePath + "_shc.asset";
                var shcAsset = AssetDatabase.LoadAssetAtPath<SHCAssetObject>(shcAssetPath);
                if(!shcAsset){
                    shcAsset = SHCAssetObject.CreateInstance<SHCAssetObject>();
                    AssetDatabase.CreateAsset(shcAsset,shcAssetPath);
                }
                shcAsset.parameters = shc;
                EditorUtility.SetDirty(shcAsset);
                AssetDatabase.SaveAssets();
            });
            System.Action callUpdate = null;
            callUpdate = ()=>{
                EditorApplication.delayCall += ()=>{
                    req.Update();
                    if(!req.done){
                        callUpdate();
                    }
                };
            };
            callUpdate();
        }

        [MenuItem("Assets/SHLearn/ReconstructTextureFromSH9")]
        public static void ReconstructCubemapFromSelectedSH9(){
            SHCAssetObject sh9 = null;
            foreach(var o in Selection.objects){
                if(o is SHCAssetObject c){
                    sh9 = c;
                    break;
                }
            }
            if(!sh9){
                return;
            }
            ReconstructCubemapFromSH9(sh9);
        }


        /// <summary>
        /// 从SH9重建CubeMap
        /// </summary>
        public static void ReconstructCubemapFromSH9(SHCAssetObject sh9){
            var sh9Array = sh9.parameters;
            var rec = new SH9Reconstruct();
            var req = rec.CreateTextureAsync(sh9Array,(rt)=>{
                var path = AssetDatabase.GetAssetPath(sh9) + ".jpg";
                SaveRenderTextureTo(rt,path);
                Debug.Log("save file to " + path);
            });
            UpdateGPUAsyncRequest(req);
        }


        [MenuItem("Assets/SHLearn/ConvertCubeMapTo6FrameLayoutTexture")]
        public static void ConvertSelectedCubeMapToTexture(){
            Cubemap cubemap = null;
            foreach(var o in Selection.objects){
                if(o is Cubemap c){
                    cubemap = c;
                    break;
                }
            }
            if(!cubemap){
                return;
            }
            ConvertCubeMapToTexture(cubemap);
        }
        private static void ConvertCubeMapToTexture(Cubemap cubemap){
            var c = new CubeMapToJPG();
            var req = c.Execute(cubemap,(tex)=>{
                var path = AssetDatabase.GetAssetPath(cubemap) + "_layout.jpg";
                SaveRenderTextureTo(tex,path);
            });
            UpdateGPUAsyncRequest(req);
        }

        public static void UpdateGPUAsyncRequest(AsyncGPUReadbackRequest req){
            EditorApplication.CallbackFunction callUpdate = null;
            callUpdate = ()=>{
                if(req.done){
                    return;
                }
                req.Update();
                EditorApplication.delayCall += callUpdate;
            };
            callUpdate();
        }

        private static void SaveRenderTextureTo(RenderTexture renderTexture,string path){
            var original = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var tex = new Texture2D(renderTexture.width,renderTexture.height,TextureFormat.RGBA32,0,false);
            tex.ReadPixels(new Rect(0,0,tex.width,tex.height),0,0,false);
            tex.Apply(false,false);
            RenderTexture.active = original;
            var bytes = tex.EncodeToJPG(100);
            System.IO.File.WriteAllBytes(path,bytes);
            AssetDatabase.Refresh();
        }
    }
}
