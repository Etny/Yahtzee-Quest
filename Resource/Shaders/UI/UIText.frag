#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D glyph;
uniform vec3 color = vec3(1.0, 0.3, 0.4);
uniform float alpha = 1;

void main()
{
	vec4 text = vec4(vec3(1.0), alpha * texture(glyph, vec2(TexCoords.x, 1.0-TexCoords.y)).r);
	FragColor = vec4(color, 1.0) * text;
}