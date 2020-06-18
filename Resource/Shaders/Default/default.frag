#version 330 core
struct Material
{
	sampler2D texture_diffuse1;
	//sampler2D texture_diffuse2;
	sampler2D texture_specular1;
	//sampler2D texture_specular2;
	sampler2D texture_normal1;
	//sampler2D texture_normal2;

	float shininess;
};

struct LightColor
{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct SpotLight
{
	vec3 position;
	vec3 direction;
	float cutoff;
	float outerCutoff;

	float constant;
    float linear;
    float quadratic;  

	bool shadowsEnabled;
	sampler2D shadowMap;
	mat4 lightSpace;

	LightColor color;
};

struct DirLight
{
	vec3 direction;
	bool shadowsEnabled;
	sampler2D shadowMap;
	mat4 lightSpace;

	LightColor color;
};

struct PointLight {    
    vec3 position;
    
    float constant;
    float linear;
    float quadratic;  

	bool shadowsEnabled;
	samplerCube shadowMap;

    LightColor color;
};  

out vec4 FragColor;

uniform Material material;

const int MaxLights = 4;

uniform int pointLightCount;
uniform int spotLightCount;
uniform int dirLightCount;

uniform float lightNearPlane;
uniform float lightFarPlane;

in VS_TAGENTLIGHTS{
	vec3 pointLightPos[MaxLights];
	vec3 spotLightPos[MaxLights];
	vec3 spotLightDir[MaxLights];
	vec3 dirLightDir[MaxLights];
} tangentLights;

uniform PointLight pointLights[MaxLights];
uniform SpotLight spotLights[MaxLights];
uniform DirLight dirLights[MaxLights];

in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} fs_in;

uniform vec3 viewPos;

vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity, bool useShadows);
vec3 CalcPointLight(int index, vec3 normal, vec3 viewDir);
vec3 CalcSpotLight(int index, vec3 normal, vec3 viewDir);
vec3 CalcDirLight(int index, vec3 normal, vec3 viewDir);  
float CalcShadow(vec4 fragPosLightSpace, in sampler2D shadowMap, bool linearizeDepth);
float CalcShadowFromCube(vec3 fragPos, vec3 lightPos, in samplerCube shadowMap);
float LinearizeLightSpaceDepth(float depth);

void main()
{
	vec3 normal = texture(material.texture_normal1, fs_in.TexCoords).rgb;
	normal = normalize(normal * 2.0 - 1.0);
	vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

	vec3 Result = vec3(0.0);
	
	for(int i = 0; i < pointLightCount; ++i)
		Result += CalcPointLight(i, normal, viewDir);

	for(int i = 0; i < dirLightCount; ++i)
		Result += CalcDirLight(i, normal, viewDir);

	for(int i = 0; i < spotLightCount; ++i)
		Result += CalcSpotLight(i, normal, viewDir);
	
	FragColor = vec4(Result, 1.0);
}

float CalcShadow(vec4 fragPosLightSpace, in sampler2D shadowMap, bool linearizeDepth)
{
    vec3 lightSpacePos = fragPosLightSpace.xyz / fragPosLightSpace.w;
    vec3 projectedFragPos = lightSpacePos * 0.5 + 0.5;

    float closestDepth = texture(shadowMap, projectedFragPos.xy).r; 
    float currentDepth = projectedFragPos.z;

	if(linearizeDepth)
	{
		closestDepth = LinearizeLightSpaceDepth(closestDepth);
		currentDepth = LinearizeLightSpaceDepth(currentDepth);
	}

    float shadow = currentDepth - .005 > closestDepth ? 0 : 1;

    return shadow;
}

float CalcShadowFromCube(vec3 fragPos, vec3 lightPos, in samplerCube shadowMap)
{
	vec3 fragToLight = fragPos - lightPos;
	
	float closest = texture(shadowMap, fragToLight).r * lightFarPlane;

	float bias = 0.01;
	float shadow = (length(fragToLight) - bias > closest) ? 0 : 1;

	return shadow;
}

float LinearizeLightSpaceDepth(float depth)
{
    float z = depth * 2.0 - 1.0;
    return (2.0 * lightNearPlane * lightFarPlane) / (lightFarPlane + lightNearPlane - z * (lightFarPlane - lightNearPlane));
}


vec3 CalcLight(vec3 lightDir, LightColor color, vec3 normal, vec3 viewDir, float attenuation, float intensity, float shadow)
{
	float diff = max(dot(normal, lightDir), 0.0);
	float spec = pow(max(dot(normal, normalize(lightDir + viewDir)), 0.0), material.shininess * 3);

	vec3 texel = vec3(texture(material.texture_diffuse1, fs_in.TexCoords));

	vec3 ambient = color.ambient * attenuation * texel;
	vec3 diffuse = color.diffuse * intensity * attenuation * diff * texel;
	vec3 specular = color.specular * spec * vec3(texture(material.texture_specular1, fs_in.TexCoords)) * intensity * attenuation;
	return vec3(ambient + (shadow * diffuse) + (shadow * specular));
}

vec3 CalcPointLight(int index, vec3 normal, vec3 viewDir)
{
	float distance = length(tangentLights.pointLightPos[index] - fs_in.TangentFragPos);
	float attenuation = 1 / (pointLights[index].constant + (pointLights[index].linear * distance) + (pointLights[index].quadratic * pow(distance, 2)));
	
	float shadow = 1;

	if(pointLights[index].shadowsEnabled)
		shadow = CalcShadowFromCube(fs_in.FragPos, pointLights[index].position, pointLights[index].shadowMap);

	return CalcLight(normalize(tangentLights.pointLightPos[index] - fs_in.TangentFragPos), pointLights[index].color, normal, viewDir, attenuation, 1.0, shadow);
}

vec3 CalcSpotLight(int index, vec3 normal, vec3 viewDir)
{
	float distance = length(tangentLights.spotLightPos[index] - fs_in.TangentFragPos);
	float attenuation = 1 / (spotLights[index].constant + (spotLights[index].linear * distance) + (spotLights[index].quadratic * pow(distance, 2)));

	vec3 lightDir = normalize(tangentLights.spotLightPos[index] - fs_in.TangentFragPos);
	float theta = dot(lightDir, normalize(-tangentLights.spotLightDir[index]));
	float epsilon = spotLights[index].cutoff - spotLights[index].outerCutoff;
	float intensity = clamp((theta - spotLights[index].outerCutoff) / epsilon, 0.0, 1.0);

	float shadow = 1;

	if(spotLights[index].shadowsEnabled && intensity > 0) 
		shadow = CalcShadow(spotLights[index].lightSpace * vec4(fs_in.FragPos, 1.0), spotLights[index].shadowMap, true);

	return CalcLight(lightDir, spotLights[index].color, normal, viewDir, attenuation, intensity, shadow);
}

vec3 CalcDirLight(int index, vec3 normal, vec3 viewDir)
{
	float shadow = 1;

	if(dirLights[index].shadowsEnabled)
		shadow = CalcShadow(dirLights[index].lightSpace * vec4(fs_in.FragPos, 1.0), dirLights[index].shadowMap, false);

	return CalcLight(normalize(-tangentLights.dirLightDir[index]), dirLights[index].color, normal, viewDir, 1.0, 1.0, shadow);
}