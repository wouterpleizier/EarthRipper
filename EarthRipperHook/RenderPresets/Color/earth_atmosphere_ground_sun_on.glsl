#if defined(GE_VERTEX_SHADER)

attribute vec3 ig_Normal;
attribute vec4 ig_Color;
attribute vec4 ig_Vertex;
attribute vec4 ig_MultiTexCoord0;

uniform mat4 ig_ModelViewMatrix;
uniform mat4 ig_ModelViewProjectionMatrix;
uniform mat4 ig_TextureMatrix;
uniform vec3 worldOriginInView;

varying vec4 vout_texCoordAndDepth;

void main()
{
	// Transform from local to view space, nullify elevation, then transform back to local.
	vec4 position = ig_ModelViewMatrix * vec4(ig_Vertex.xyz, 1.0);
	position.xyz -= worldOriginInView;
	position.xyz = normalize(position.xyz);
	position.xyz += worldOriginInView;
	position = inverse(ig_ModelViewMatrix) * position;
	
	// Same as original shader (see InitGlobals() and AtmosphereGroundSunOnVertexShader() in atmosphere.glslesv).
	position = ig_ModelViewProjectionMatrix * position;
	position.z = min(position.z, position.w * 0.99999);
	gl_Position = position;

	vec4 transformedTexCoord = ig_TextureMatrix * vec4(ig_MultiTexCoord0.xy, 0.0, 1.0);
	vout_texCoordAndDepth.xyz = transformedTexCoord.xyw;

	// Now that everything has the same elevation, Z-fighting may occur in areas with photogrammetry. To work around
	// this, we'll pass along the depth value that would've resulted from leaving the elevation intact.
	vec4 defaultPosition = ig_ModelViewProjectionMatrix * vec4(ig_Vertex.xyz, 1.0);
	vout_texCoordAndDepth.w = min(defaultPosition.z, defaultPosition.w * 0.99999);
}

#endif // defined(GE_VERTEX_SHADER)

//----------------------------------------------------------------------------------------------------------------------

#if defined(GE_PIXEL_SHADER)

uniform sampler2D groundTexture;

varying vec4 vout_texCoordAndDepth;

void main()
{
	gl_FragColor = texture2D(groundTexture, vout_texCoordAndDepth.st);
	gl_FragDepth = vout_texCoordAndDepth.w;
}

#endif // defined(GE_PIXEL_SHADER)
