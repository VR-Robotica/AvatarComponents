// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VR Robotica/Unlit_Mouth" 
{
    Properties 
	{
        _LipTex		("Lip Texture",   2D)	= "white" {}
        _TeethTex	("Teeth Texture", 2D)	= "white" {}
        _MouthTex	("Mouth Texture", 2D)	= "white" {}
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
            uniform sampler2D _LipTex; uniform float4 _LipTex_ST;
            uniform sampler2D _TeethTex; uniform float4 _TeethTex_ST;
            uniform sampler2D _MouthTex; uniform float4 _MouthTex_ST;
			            
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
                float4 _LipTex_var = tex2D(_LipTex,TRANSFORM_TEX(i.uv0, _LipTex));
                clip(_LipTex_var.a - 0.5);

                float4 _TeethTex_var = tex2D(_TeethTex,TRANSFORM_TEX(i.uv0, _TeethTex));
                float4 _MouthTex_var = tex2D(_MouthTex,TRANSFORM_TEX(i.uv0, _MouthTex));
                float3 node_2454 = saturate((_LipTex_var.rgb*saturate(((1.0-saturate((saturate((1.0-(1.0-_TeethTex_var.rgb)))+_TeethTex_var.rgb-1.0)))))));
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
            uniform sampler2D _LipTex; uniform float4 _LipTex_ST;
            
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
                float4 _LipTex_var = tex2D(_LipTex,TRANSFORM_TEX(i.uv0, _LipTex));
                clip(_LipTex_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
