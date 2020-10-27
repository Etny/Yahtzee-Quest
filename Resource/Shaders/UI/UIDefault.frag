#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform vec3 color = vec3(0.5, 0.5, 0.5);

void main()
{
	FragColor = vec4(color, 1);
}