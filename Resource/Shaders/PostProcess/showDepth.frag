#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;

void main()
{
	FragColor = vec4(texture(screen, TexCoords).rgb, 1);
}