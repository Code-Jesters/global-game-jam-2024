Shader "Custom/Hair"
{
    SubShader
    {
        Tags
        {
            "LightMode" = "ForwardBase"
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct vertexData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            int _ShellIndex; // This is the current shell layer being operated on, it ranges from 0 -> _ShellCount 
			int _ShellCount; // This is the total number of shells, useful for normalizing the shell index
			float _ShellLength; // This is the amount of distance that the shells cover, if this is 1 then the shells will span across 1 world space unit
			float _Density;  // This is the density of the strands, used for initializing the noise
			float _NoiseMin, _NoiseMax; // This is the range of possible hair lengths, which the hash then interpolates between 
			float _Thickness; // This is the thickness of the hair strand
			float _Attenuation; // This is the exponent on the shell height for lighting calculations to fake ambient occlusion (the lack of ambient light)
			float _OcclusionBias; // This is an additive constant on the ambient occlusion in order to make the lighting less harsh and maybe kind of fake in-scattering
			float _ShellDistanceAttenuation; // This is the exponent on determining how far to push the shell outwards, which biases shells downwards or upwards towards the minimum/maximum distance covered
			float _Curvature; // This is the exponent on the physics displacement attenuation, a higher value controls how stiff the hair is
			float _DisplacementStrength; // The strength of the displacement (very complicated)
			float3 _ShellColor; // The color of the shells (very complicated)
			float3 _ShellDirection; // The direction the shells are going to point towards, this is updated by the CPU each frame based on user input/movement


            float hash(uint n)
            {
	            n = (n << 13U) ^ n;
            	n = n * (n * n * 15731U + 0x7892221U) + 0x1376312589U;
            	return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
            }

            v2f vert(vertexData vd)
            {
                v2f i;
            	float shellHeight = (float)_ShellIndex / (float)_ShellCount;

            	shellHeight = pow(shellHeight, _ShellDistanceAttenuation);

            	vd.vertex.xyz += vd.normal.xyz * _ShellLength * shellHeight;

            	i.normal = normalize(UnityObjectToWorldNormal(vd.normal));

            	float k = pow(shellHeight, _Curvature);

            	vd.vertex.xyz += _ShellDirection * k * _DisplacementStrength;

            	i.worldPos = mul(unity_ObjectToWorld, vd.vertex);
            	i.pos = UnityObjectToClipPos(vd.vertex);

            	i.uv = vd.uv;

            	return i;
            }
            
            float4 frag(v2f i) : SV_Target{
            	float2 newUV = i.uv * _Density;

            	float2 localUV = frac(newUV) * 2 - 1;

            	float localDistanceFromCenter = length(localUV);

            	uint2 tid = newUV;
            	uint seed = tid.x + 100 * tid.y + 100 * 10;

            	float shellIndex = _ShellIndex;
            	float shellCount = _ShellCount;

            	float rand = lerp(_NoiseMin, _NoiseMax, hash(seed));

            	float h = shellIndex / shellCount;

            	int outsideThickness = (localDistanceFromCenter) > (_Thickness * (rand - h));

            	if(outsideThickness && shellIndex > 0) discard;

            	float ndot1 = dot(i.normal, _WorldSpaceLightPos0) * 0.5f + 0.5f;

            	ndot1 = ndot1 * ndot1;

            	float ambientOcclusion = pow(h, _Attenuation);

            	ambientOcclusion += _OcclusionBias;

            	ambientOcclusion = saturate(ambientOcclusion);

            	return float4(_ShellColor * ndot1 * ambientOcclusion, 1.0);
            }
            
            ENDCG
        }
    }
}
