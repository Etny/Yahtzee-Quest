#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform vec3 Color = vec3(1.0);

void main()
{
	//gl_FragDepth = 0.0;
	FragColor = vec4(Color, 1);
}