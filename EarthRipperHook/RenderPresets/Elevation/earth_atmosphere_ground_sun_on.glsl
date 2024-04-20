#if defined(GE_VERTEX_SHADER)

const float minElevationInKm = -1.0;
const float maxElevationInKm = 9.0;

attribute vec3 ig_Normal;
attribute vec4 ig_Color;
attribute vec4 ig_Vertex;
attribute vec4 ig_MultiTexCoord0;

uniform mat4 ig_ModelViewMatrix;
uniform mat4 ig_ModelViewProjectionMatrix;
uniform mat4 ig_TextureMatrix;
uniform vec3 worldOriginInView;

varying float vout_elevationNormalized;

void main()
{
	// Transform from local to view space, store and nullify elevation, then transform back to local.
	vec4 position = ig_ModelViewMatrix * vec4(ig_Vertex.xyz, 1.0);
	position.xyz -= worldOriginInView;

	float elevation = length(position.xyz) - 1.0;
	float elevationInKm = elevation * 6360.0;
	vout_elevationNormalized = clamp((elevationInKm - minElevationInKm) / (maxElevationInKm - minElevationInKm), 0.0, 1.0);

	position.xyz = normalize(position.xyz);
	position.xyz += worldOriginInView;
	position = inverse(ig_ModelViewMatrix) * position;
	
	// Same as original shader (see InitGlobals() and AtmosphereGroundSunOnVertexShader() in atmosphere.glslesv).
	position = ig_ModelViewProjectionMatrix * position;
	position.z = min(position.z, position.w * 0.99999);
	gl_Position = position;
}

#endif // defined(GE_VERTEX_SHADER)

//----------------------------------------------------------------------------------------------------------------------

#if defined(GE_PIXEL_SHADER)

varying float vout_elevationNormalized;

vec2 normalizedFloatToUShort(float value)
{
	value = clamp(value, 0.0, 1.0) * 65535.0;
	float r = floor(value / 256.0);
	float g = value - (r * 256.0);

	return vec2(r, g) / 255.0;
}

void main()
{
	#if defined(EARTHRIPPER_CAPTURE)
		vec2 rg = normalizedFloatToUShort(vout_elevationNormalized);
		gl_FragColor = vec4(rg.r, rg.g, 0.0, 1.0);
	#else
		gl_FragColor = vec4(vout_elevationNormalized, vout_elevationNormalized, vout_elevationNormalized, 1.0);
	#endif

	gl_FragDepth = 1.0 - vout_elevationNormalized;
}

#endif // defined(GE_PIXEL_SHADER)
