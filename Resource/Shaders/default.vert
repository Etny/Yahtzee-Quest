#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;


layout (std140) uniform Matrices
{
	mat4 projection;
	mat4 view;
};

uniform mat4 model;
uniform vec3 viewPos;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} vs_out;

void main()
{
	gl_Position = projection * view * model * vec4(aPos, 1.0);
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	//vs_out.Normal = mat3(transpose(inverse(model))) * aNormal;
	vs_out.TexCoords = vec2(aTexCoords.x, aTexCoords.y);

	
	
}
