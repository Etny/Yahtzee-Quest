#version 330 core
layout (location = 0) in vec3 aPos;

uniform mat4 lightSpace;
uniform mat4 models[100];

out vec4 FragPos;

void main()
{
	mat4 model = models[gl_InstanceID];
	gl_Position = lightSpace * model * vec4(aPos, 1);
	FragPos = model * vec4(aPos, 1.0);
}