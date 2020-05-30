#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

/*CBUFFER_START(_CustomLight)
	float3 _DirectionalLightColor;
	float3 _DirectionalLightDirection;
CBUFFER_END*/

float4 _DirectionalLightColor = 1.0;
float3 _DirectionalLightDirection = 1.0;

struct Light {
	float3 color;
	float3 direction;
};

Light GetDirectionalLight() {
	Light light;
	light.color = _DirectionalLightColor.rgb;
	light.direction = _DirectionalLightDirection.xyz;
	//light.color = 1.0;
	//light.direction = 1.0;
	return light;
}

#endif