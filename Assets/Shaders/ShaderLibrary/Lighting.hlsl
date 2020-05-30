#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 remap(float3 input, float min1, float max1, float min2, float max2) {

	float perc = (input - min1) / (max1 - min1);
	float value = perc * (max2 - min2) + min2;

	return value;
}

float3 posterize(float3 input, float step) {
	return round(input * step) / step;
}

float3 IncomingLight(Surface surface, Light light) {
	//return saturate(dot(surface.normal, light.direction)) * light.color;
	//return posterize(remap(dot(surface.normal, light.direction), -1.0, 1.0, 0.0, 1.0), 5.0) * light.color;
	return remap(dot(surface.normal, light.direction), -1.0, 1.0, 0.0, 1.0) * light.color;
}

float3 GetLighting(Surface surface, Light light) {
	return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting(Surface surface) {
	return GetLighting(surface, GetDirectionalLight()); 
}

#endif