Shader "PostProcess/Fog"
{
    Properties
    {
        _FogColor("Fog Color", Color) = (1,1,1)

        _FogDensity("Fog Density", Range(0.0, 1.0)) = 0.5
        _FogStart("Fog Start", Float) = 0.5
        _FogEnd("Fog End", Float) = 0.5
        _FogDeepStart("Fog Deep Start", Float) = 0.5
        _FogDeepEnd("Fog Deep End", Float) = 0.5
    }

    SubShader
    {

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        ENDHLSL
        Blend SrcAlpha OneMinusSrcAlpha
        //        Blend One Zero
        ZWrite Off
        ZTest Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _  _ENABLE_DEEP_FOG
            #pragma multi_compile _ _ENALBE_FAR_FOG

            float4 _FogColor;
            float _FogDensity;
            float _FogStart;
            float _FogEnd;
            float _FogDeepStart;
            float _FogDeepEnd;
            float4 _CameraForward;
            float4x4 _ClipToWorldMatrix;
            float _NearPlane;
            float4 _CameraWorldPos;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                // #if defined(_ENABLE_DEEP_FOG)
                float3 farPlaneWS:TEXCOORD1;
                float3 direction:TEXCOORD2;
                // #endif
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                float depth = 0;
                #if UNITY_REVERSED_Z
                depth = 1;
                #endif

                float4 pos = mul(_ClipToWorldMatrix, float4((input.uv * 2 - 1), 1, 1));
                output.farPlaneWS = pos.xyz / pos.w;
                output.direction = (output.farPlaneWS - _CameraWorldPos.xyz);
                // #endif
                return output;
            }

            half4 frag(Varyings s):SV_Target
            {
                real depth = SampleSceneDepth(s.uv);

                float d = 0;
                #if defined(_ENALBE_FAR_FOG)
                    if(unity_OrthoParams.w != 1.0)
                    {
                        float z = Linear01Depth(depth, _ZBufferParams);
                        d =  smoothstep(_FogStart,_FogEnd,z);
                    }
                    else
                    {
                        #if (UNITY_REVERSED_Z == 1)
                                        depth = 1 - depth;
                        #endif
                        d =  smoothstep(_FogStart,_FogEnd,depth);
                    }
                #endif
                // return s.farPlaneWS.y;
                float3 posWS;
                #if defined(_ENABLE_DEEP_FOG)
                    
                    if(unity_OrthoParams.w == 1.0)
                    {
                        #if (UNITY_REVERSED_Z != 1)
                                        depth = 1 - depth;
                        #endif
                        float z =   -(_ProjectionParams.z - _ProjectionParams.y) * depth ;
                        posWS = s.farPlaneWS + z * _CameraForward;
                    }
                    else
                    {
                        float z =  Linear01Depth(depth,_ZBufferParams) ;
                        posWS = _CameraWorldPos.xyz + z *  (s.direction);
                    }
                     
                    d = 1-smoothstep(_FogDeepStart,_FogDeepEnd,posWS.y);
                #endif
                // return float4(s.farPlaneWS,1);
                // return posWS.y;
                #if !defined(_ENABLE_DEEP_FOG) && !defined(_ENALBE_FAR_FOG)
                d = 0;
                #endif

                d += saturate(d) * _FogDensity;
                return half4(_FogColor.rgb, d);
            }
            ENDHLSL

        }

    }

}