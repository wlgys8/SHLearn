using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SHLearn{

    [ExecuteInEditMode]
    public class SHExample : MonoBehaviour
    {

        [SerializeField]
        Cubemap _skybox;

        private Material _matOfSphere;

        [SerializeField]
        private Renderer _sphere;

        private Material _skyboxMat;

        private AsyncGPUReadbackRequest _gpuReq;

        [SerializeField]
        private List<Vector4> _sh9;

        private bool _isGPUCalculating = false;

        [ContextMenu("Bake")]
        public void Bake(){
            this.UpdateSkybox();
            var proj = new SH9Project();
            _isGPUCalculating = true;
            _gpuReq = proj.FromCubeMapAsync(this._skybox,(sh9)=>{
                _isGPUCalculating = false;
                _sh9 = new List<Vector4>(sh9);
                if(_matOfSphere){
                     this.UpdateSphere(_sh9);
                }
            });
        }

        public AsyncGPUReadbackRequest gpuRequest{
            get{
                return _gpuReq;
            }
        }

        private void UpdateSphere(List<Vector4> shc){
            if(!_matOfSphere){
                _matOfSphere = new Material(Shader.Find("SHLearn/SHDiffuse"));
            }
            _matOfSphere.SetVectorArray("_shc",shc);
            if(this._sphere){
                _sphere.sharedMaterial = _matOfSphere;
            }
        }

        private void UpdateSkybox(){
            if(!_skyboxMat){
                _skyboxMat = new Material(Shader.Find("Skybox/Cubemap"));
            }
            var cubemap = _skyboxMat.GetTexture("_Tex");
            if(cubemap != _skyboxMat){
                _skyboxMat.SetTexture("_Tex",_skybox);
            }
            RenderSettings.skybox = _skyboxMat;
        }

        public bool CheckGPUCalculating(){
            if(_isGPUCalculating){
                _gpuReq.Update();
            }
            if(_gpuReq.done){
                _isGPUCalculating = false;
            }
            return _isGPUCalculating;
        }

        void Update(){
            CheckGPUCalculating();
            if(_sh9 != null){
                if(!_matOfSphere || !_matOfSphere.HasProperty("_shc")){
                    this.UpdateSphere(_sh9);
                }
            }
        }
    }
}
