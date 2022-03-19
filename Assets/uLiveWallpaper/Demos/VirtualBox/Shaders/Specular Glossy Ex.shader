// Simplified Bumped Specular shader. Differences from regular Bumped Specular one:
// - no Main Color nor Specular Color
// - specular lighting directions are approximated per vertex
// - writes zero to alpha channel
// - no Deferred Lighting support
// - no Lightmap support
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Hidden/uLiveWallpaper/Specular Glossy" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_ShininessGlossScaleFactor ("ShininessGlossScaleFactor", Range (0.03, 1)) = 0.078125
	_GlossMultiplier ("Gloss Multipliers", Range (0.1, 10)) = 2
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap nofog noforwardadd halfasview interpolateview noambient 

inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	fixed nh = max (0, dot (s.Normal, halfDir));
	fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
	UNITY_OPAQUE_ALPHA(c.a);
	return c;
}

sampler2D _MainTex;
sampler2D _BumpMap;
half _Shininess;
half _GlossMultiplier;
half _ShininessGlossScaleFactor;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = tex.rgb;
	o.Gloss = 1 + tex.a * _GlossMultiplier;
	o.Alpha = tex.a;
	o.Specular = lerp(_Shininess, _Shininess * _ShininessGlossScaleFactor, tex.a);
}
ENDCG
}

FallBack "Mobile/VertexLit"
}
