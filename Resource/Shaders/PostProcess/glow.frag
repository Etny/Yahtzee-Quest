#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;

uniform vec2 GlowCenter;
uniform vec3 Color;
uniform float MaxDistance;
uniform bool Enabled;

void main()
{
	if(!Enabled)
	{
		FragColor =texture(screen, TexCoords);
		return;
	}

	float dist = clamp(1.0 - (distance(gl_FragCoord.xy, GlowCenter) / 100), 0.0, 1.0);
	vec4 glow = vec4(dist * Color, 1);

	FragColor = glow + texture(screen, TexCoords);
}