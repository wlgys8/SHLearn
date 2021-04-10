Shader "SHLearn/SHDiffuse"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "./SHCommon.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal: NORMAL;
                float4 tangent:TANGENT;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normalWS :TEXCOORD1 ;
            };

            float4 _shc[9];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 posWS = mul(unity_ObjectToWorld,v.vertex);
                float3 normalWS = UnityObjectToWorldNormal(v.normal);
                o.normalWS = normalWS;
                return o;
            }

            half4 SH9(float3 dir){
                float3 d = float3(dir.x,dir.z,dir.y);
                float4 color = 
                _shc[0] * GetY00(d) + 
                _shc[1] * GetY1n1(d) + 
                _shc[2] * GetY10(d) + 
                _shc[3] * GetY1p1(d) + 
                _shc[4] * GetY2n2(d) + 
                _shc[5] * GetY2n1(d) + 
                _shc[6] * GetY20(d) + 
                _shc[7] * GetY2p1(d) + 
                _shc[8] * GetY2p2(d);
                return color;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normalWS = i.normalWS;
                return SH9(normalWS);
            }
            ENDCG
        }
    }
}
