#version 330 core

out vec4 FragColor;

uniform bool overlapping = false;

void main()
{
	if(!overlapping) FragColor = vec4(1, 0.9, 0, 1);
	else FragColor = vec4(1, .3, 0, 1);
}
