using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace SHLearn{
    public class SH9Project
    {

        private const int SAMPLE_SIZE_X = 512;
        private const int SAMPLE_SIZE_Y = 512;
        private const int THREAD_X = 8;
        private const int THREAD_Y = 8;
        private const int GROUP_X = SAMPLE_SIZE_X / THREAD_X;
        private const int GROUP_Y = SAMPLE_SIZE_Y / THREAD_Y;
        private const int SHC_COUNT = 9;
        private static ComputeShader _computeShader;

        private static ComputeShader computeShader{
            get{
                if(!_computeShader){
                    _computeShader = Resources.Load<ComputeShader>("SH9ProjectFromCubeMap");
                }
                return _computeShader;
            }
        }

        public SH9Project(){
        }

        public AsyncGPUReadbackRequest FromCubeMapAsync(Cubemap cubemap,System.Action<Vector4[]> callback){
            var shcBuffer = new ComputeBuffer(GROUP_X * GROUP_Y * SHC_COUNT,16);
            computeShader.SetTexture(0,"CubeMap",cubemap);
            computeShader.SetBuffer(0,"shcBuffer",shcBuffer);
            computeShader.SetInts("SampleSize",SAMPLE_SIZE_X,SAMPLE_SIZE_Y);
            computeShader.Dispatch(0,GROUP_X,GROUP_Y,1);
            return AsyncGPUReadback.Request(shcBuffer,(req)=>{
                if(req.hasError){
                    Debug.LogError("sh project with gpu error");
                    shcBuffer.Release();
                    callback(null);
                    return;
                }
                var groupShc = req.GetData<Vector4>();
                var count = groupShc.Length / SHC_COUNT;
                var shc = new Vector4[SHC_COUNT];
                for(var i = 0; i < count; i ++){
                    for(var offset = 0; offset < SHC_COUNT; offset ++){
                        shc[offset] += groupShc[i * SHC_COUNT + offset];
                    }
                }
                shcBuffer.Release();
                callback(shc);
            });
        }
    }
}
