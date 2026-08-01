Shader "RunGame/Plus Particle"
{
    Properties { _Color ("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            half4 _Color;
            Varyings vert(Attributes input) { Varyings output; output.positionHCS = TransformObjectToHClip(input.positionOS.xyz); output.uv = input.uv; output.color = input.color * _Color; return output; }
            half4 frag(Varyings input) : SV_Target
            {
                float2 p = abs(input.uv - 0.5);
                half vertical = (1.0 - smoothstep(0.10, 0.15, p.x)) * (1.0 - smoothstep(0.36, 0.43, p.y));
                half horizontal = (1.0 - smoothstep(0.10, 0.15, p.y)) * (1.0 - smoothstep(0.36, 0.43, p.x));
                return half4(input.color.rgb, input.color.a * max(vertical, horizontal));
            }
            ENDHLSL
        }
    }
}
