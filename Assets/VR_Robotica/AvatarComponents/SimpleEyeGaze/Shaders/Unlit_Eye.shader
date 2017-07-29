Shader "VR Robotica/Unlit_Eye" 
{
    Properties 
	{
        _TopTex ("TopTex", 2D) = "white" {}
        _IrisMask ("IrisMask", 2D) = "white" {}
        _BotTex ("BotTex", 2D) = "white" {}
        _IrisColor ("IrisColor", Color) = (1,0,0,1)
        _Highlights ("Highlights", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }

    SubShader 
	{
        Tags 
		{
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="TransparentCutout"
        }
		Pass 
		{
            Name "FORWARD"
            Tags 
			{
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _TopTex; uniform float4 _TopTex_ST;
            uniform sampler2D _IrisMask; uniform float4 _IrisMask_ST;
            uniform sampler2D _BotTex; uniform float4 _BotTex_ST;
            uniform float4 _IrisColor;
            uniform sampler2D _Highlights; uniform float4 _Highlights_ST;
            struct VertexInput 
			{
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput 
			{
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR 
			{
                float4 _TopTex_var = tex2D(_TopTex,TRANSFORM_TEX(i.uv0, _TopTex));
                clip(_TopTex_var.a - 0.5);

                float4 _Highlights_var = tex2D(_Highlights,TRANSFORM_TEX(i.uv0, _Highlights));
                float4 _IrisMask_var = tex2D(_IrisMask,TRANSFORM_TEX(i.uv0, _IrisMask));
                float4 _BotTex_var = tex2D(_BotTex,TRANSFORM_TEX(i.uv0, _BotTex));
                float3 node_2454 = saturate((_TopTex_var.rgb*saturate((1.0-(1.0-_Highlights_var.rgb)*(1.0-saturate((saturate((1.0-(1.0-_IrisMask_var.rgb)*(1.0-_IrisColor.rgb)))+_BotTex_var.rgb-1.0)))))));
                float3 emissive = node_2454;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
		Pass 
		{
            Name "ShadowCaster"
            Tags 
			{
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _TopTex; uniform float4 _TopTex_ST;
            struct VertexInput 
			{
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput 
			{
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR 
			{
                float4 _TopTex_var = tex2D(_TopTex,TRANSFORM_TEX(i.uv0, _TopTex));
                clip(_TopTex_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
