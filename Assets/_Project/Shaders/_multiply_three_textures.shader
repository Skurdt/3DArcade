Shader "Unlit/TexturesMultiply_3"
{
    Properties
    {
        [PerRendererData] _BaseMap ("BaseMap", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 4.0
        _Mask ("Mask", 2D) = "white" {}
        _Scanlines ("Scanlines", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_mask : TEXCOORD1;
                float2 uv_scan : TEXCOORD2;
            };

            struct v2f
            {
                float4 vertex  : SV_POSITION;
                float2 uv      : TEXCOORD0;
                float2 uv_mask : TEXCOORD1;
                float2 uv_scan : TEXCOORD2;
            };

            sampler2D _BaseMap;
            sampler2D _Mask;
            sampler2D _Scanlines;
            float4 _BaseMap_ST;
            float4 _Mask_ST;
            float4 _Scanlines_ST;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex  = UnityObjectToClipPos(v.vertex);
                o.uv      = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uv_mask = TRANSFORM_TEX(v.uv_mask, _Mask);
                o.uv_scan = TRANSFORM_TEX(v.uv_scan, _Scanlines);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col       = tex2D(_BaseMap, i.uv) * _Intensity;
                fixed4 mask      = tex2D(_Mask, i.uv_mask);
                fixed4 scanlines = tex2D(_Scanlines, i.uv_scan);
                return col * mask * scanlines;
            }
            ENDCG
        }
    }
}
