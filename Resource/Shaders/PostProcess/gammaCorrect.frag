#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;

void main()
{
	FragColor = texture(screen, TexCoords);
	FragColor.rgb = pow(FragColor.rgb, vec3(1.0/2.2));
}