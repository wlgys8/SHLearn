using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace SHLearn{
    public class SH9Reconstruct
    {
        private static ComputeShader _computeShader;

        private static ComputeShader computeShader{
            get{
                if(!_computeShader){
                    _computeShader = Resources.Load<ComputeShader>("SH9ReconstructToTexture");
                }
                return _computeShader;
            }
        }
        private const int CUBE_FACE_SIZE = 512;

        private const int THREAD_X = 8;
        private const int THREAD_Y = 8;
        private const int GROUP_X = CUBE_FACE_SIZE / THREAD_X; //64
        private const int GROUP_Y = CUBE_FACE_SIZE / THREAD_X; //64
        private const int GROUP_Z = 6;

        public SH9Reconstruct(){
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


        /// <summary>
        /// 利用球谐参数重建环境贴图
        /// </summary>
        public AsyncGPUReadbackRequest CreateTextureAsync(Vector4[] sh9,System.Action<RenderTexture> callback){
            var renderTex = CreateRenderTexture();
            computeShader.SetVectorArray("shC",sh9);
            computeShader.SetTexture(0,"Result",renderTex);
            computeShader.SetInt("faceSize",CUBE_FACE_SIZE);
            computeShader.Dispatch(0,GROUP_X,GROUP_Y,GROUP_Z);
            return AsyncGPUReadback.Request(renderTex,0,0,renderTex.width,0,renderTex.height,0,1,(res)=>{
                if(res.hasError){
                    Debug.LogError("sh9 reconstruct with gpu error");
                }
                callback(renderTex);
            });
        }
    }
}
