#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;
layout (location = 3) in vec3 aTangent;
layout (location = 4) in vec3 aBitangent;


layout (std140) uniform Matrices
{
	mat4 projection;
	mat4 ortho;
	mat4 view;
};

const int MaxLights = 4;

struct Light
{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;

	int type;

	vec3 position;
	vec3 direction;

	float cutoff;
	float outerCutoff;

	float constant;
    float linear;
    float quadratic;

	bool shadowsEnabled;
	int shadowIndex;
	mat4 lightSpace;
};

uniform int lightCount;
uniform Light Lights[MaxLights];

uniform mat4 models[100];
uniform vec3 viewPos;

out VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} vs_out;

out TangentData{
	vec3 tangentDir[MaxLights];
	vec3 tangentPos[MaxLights];
} tangentData;

uniform bool RenderOrtho = false;

void main()
{
	mat4 model = models[gl_InstanceID];
	mat4 project = RenderOrtho ? ortho : projection;
	gl_Position = project * view * model * vec4(aPos, 1.0);
	vs_out.FragPos = vec3(model * vec4(aPos, 1.0));
	vs_out.TexCoords = vec2(aTexCoords.x, aTexCoords.y);

	vec3 T = normalize(vec3(model * vec4(aTangent, 0.0)));
	vec3 B = normalize(vec3(model * vec4(aBitangent, 0.0)));
	vec3 N = normalize(vec3(model * vec4(aNormal, 0.0)));
	mat3 TBN = transpose(mat3(T, B, N));

	vs_out.TBN = TBN;

	vs_out.TangentFragPos = TBN * vs_out.FragPos;
	vs_out.TangentViewPos = TBN * viewPos;

	for(int i = 0; i < lightCount; ++i)
	{
		tangentData.tangentDir[i] = TBN * Lights[i].direction;
		tangentData.tangentPos[i] = TBN * Lights[i].position;
	}
}
