using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace SHLearn{
    public class CubeMapToJPG
    {
        private static ComputeShader _computeShader;

        private static ComputeShader computeShader{
            get{
                if(!_computeShader){
                    _computeShader = Resources.Load<ComputeShader>("CubeMapTo6FrameTexture");
                }
                return _computeShader;
            }
        }
        private const int CUBE_FACE_SIZE = 512;
        private const int GROUP_X = CUBE_FACE_SIZE / 8; //64
        private const int GROUP_Y = CUBE_FACE_SIZE / 8; //64
        private const int GROUP_Z = 6;

        public CubeMapToJPG(){
        }

        private RenderTexture CreateRenderTexture(){
            var preferRTWidth = CUBE_FACE_SIZE * 4;
            var preferRTHeight = CUBE_FACE_SIZE * 3;
            var rtDes = new RenderTextureDescriptor(preferRTWidth,preferRTHeight,GraphicsFormat.R8G8B8A8_UNorm,0);
            rtDes.enableRandomWrite = true;
            var ret = new RenderTexture(rtDes);
            ret.Create();
            return ret;
        }

        public AsyncGPUReadbackRequest Execute(Cubemap cubemap,System.Action<RenderTexture> callback){
            var renderTex = CreateRenderTexture();
            computeShader.SetTexture(0,"_CubeMap",cubemap);
            computeShader.SetTexture(0,"Result",renderTex);
            computeShader.Dispatch(0,GROUP_X,GROUP_Y,GROUP_Z);
            return AsyncGPUReadback.Request(renderTex,0,0,renderTex.width,0,renderTex.height,0,1,(res)=>{
                if(res.hasError){
                    Debug.LogError("AsyncGPUReadback Error");
                }
                callback(renderTex);
            });
        }
    }
}
