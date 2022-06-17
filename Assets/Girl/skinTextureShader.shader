Shader "Custom/skinTextureShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Emission("Emission", Color) = (0,0,0,0)
        _MakeupMultiplier("MakeupMultiplier", Range(0,1)) = 1.0
        _TFGradient("_TFGradient", 2D) = "black" {}
        _FurTex("_FurTex", 2D) = "black" {}
        _FurTF("FurTF", Range(-3.5,.5)) = 1.0
        _FurBaseColor("_FurBaseColor", Color) = (.5,.4,.2,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
//#pragma vertex vert

        //#include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _TFGradient;
        sampler2D _FurTex;

        struct Input
        {
            //float2 uv_MainTex;
            float2 uv_MainTex : TEXCOORD0;
            //float2 uv_TF : TEXCOORD3;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half3 _Emission;
        float _MakeupMultiplier;
        float _FurTF;
        float4 _FurBaseColor;

        //Input vert(inout appdata_full v)
        //{
        //    Input o;
        //    o.uv_TF = v.TEXCOORD0; 
        //    return o;
        //}

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float4 t = tex2D(_MainTex, IN.uv_MainTex);
            t.xyz = t.xyz * .7;
            //fixed4 c = float4( (t.xyz * t.a) + _Color.xyz, 1);
            //fixed4 c = float4( lerp(_Color.xyz, t.xyz, t.a * _MakeupMultiplier), 1);
            fixed4 c = _Color;

            float edgeSize = .05;
            //float TFPercent = .25;

            float g = tex2D(_TFGradient, IN.uv_MainTex).b * 10 - 9.7;
            
            //c = g * (1 / edgeSize) - (TFPercent * (1 / edgeSize));
            float TF = g * (1 / edgeSize) - (_FurTF * (1 / edgeSize));
            float4 furt = tex2D(_FurTex, IN.uv_MainTex);
            //furt.xyz = furt.xyz * 1.2;
            float4 fur = lerp(_FurBaseColor, furt, furt.a);
            fur.a = 1;
            c = lerp(c, fur, saturate(TF));

            c = float4(lerp(c, t.xyz, t.a * _MakeupMultiplier), 1);
            //c = fur;

            //c.xy = IN.uv_MainTex;
            //c.xy = normalize(IN.uv_TF.xy);
            //c = float4(1, 0, 0, 1);

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic - 0;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Emission = _Emission;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
