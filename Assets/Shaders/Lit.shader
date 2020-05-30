Shader "Mobile RP/Lit" {

	Properties{
		_BaseMap("Texture", 2D) = "white" {}
		_BaseColor("Color", Color) = (0.5, 0.5, 0.5, 1.0)
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src BLend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst BLend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite("Z Write", Float) = 1
	}

		SubShader{
			Tags { "LightMode" = "CustomLit" }
			Pass {
				Blend[_SrcBlend][_DstBlend]
				ZWrite[_ZWrite]

				HLSLPROGRAM
				#pragma multi_compile_instancing
				#pragma vertex LitPassVertex
				#pragma fragment LitPassFragment
				#include "LitPass.hlsl"
				ENDHLSL
			}
		}
}