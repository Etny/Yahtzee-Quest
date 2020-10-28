#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D image;
uniform vec4 tint = vec4(0);

void main()
{
	FragColor = mix(texture2D(image, TexCoords), tint, tint.a);
}