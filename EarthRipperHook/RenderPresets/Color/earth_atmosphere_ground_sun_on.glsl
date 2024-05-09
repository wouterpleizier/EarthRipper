#extension GL_EXT_frag_depth : enable

#if defined(GL_ES)
precision highp float;
#endif

varying vec4 vout_texCoordAndDepth;

#if defined(GE_VERTEX_SHADER)

attribute vec3 ig_Normal;
attribute vec4 ig_Color;
attribute vec4 ig_Vertex;
attribute vec4 ig_MultiTexCoord0;

uniform mat4 ig_ModelViewMatrix;
uniform mat4 ig_ModelViewProjectionMatrix;
uniform mat4 ig_TextureMatrix;
uniform vec3 worldOriginInView;

mat4 invertMatrix(mat4 m)
{
	// The built-in inverse() function works in OpenGL mode but does not transpile to HLSL/DirectX, so we'll use this
	// implementation instead. (Ideally, we wouldn't be inverting matrices within shaders at all, but this saves us the
	// trouble of messing with the uniforms that are provided to us by default)
	// Adapted from: https://gist.github.com/mattatz/86fff4b32d198d0928d0fa4ff32cf6fa
	
    float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
    float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
    float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
    float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

    float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
    float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
    float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
    float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

    float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
    float idet = 1.0 / det;

    mat4 ret;

    ret[0][0] = t11 * idet;
    ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
    ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
    ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

    ret[1][0] = t12 * idet;
    ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
    ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
    ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

    ret[2][0] = t13 * idet;
    ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
    ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
    ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

    ret[3][0] = t14 * idet;
    ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
    ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
    ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

    return ret;
}

void main()
{
	// Transform from local to view space, nullify elevation, then transform back to local.
	vec4 position = ig_ModelViewMatrix * vec4(ig_Vertex.xyz, 1.0);
	position.xyz -= worldOriginInView;
	position.xyz = normalize(position.xyz);
	position.xyz += worldOriginInView;
	position = invertMatrix(ig_ModelViewMatrix) * position;
	
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

void main()
{
	gl_FragColor = texture2D(groundTexture, vout_texCoordAndDepth.st);
	gl_FragDepthEXT = vout_texCoordAndDepth.w;
}

#endif // defined(GE_PIXEL_SHADER)
