#version 330 core
struct Material
{
	sampler2D texture_diffuse1;
	sampler2D texture_diffuse2;
	sampler2D texture_specular1;
	sampler2D texture_specular2;
	sampler2D texture_normal1;
	sampler2D texture_normal2;

	float shininess;
};


out vec4 FragColor;

uniform Material material;

in VS_OUT{
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} fs_in;

void main()
{
	FragColor = texture(material.texture_diffuse1, fs_in.TexCoords);
}
