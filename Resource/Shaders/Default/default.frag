#version 330 core
struct Material
{
	bool usingDiffuseMap;
	vec3 diffuseColor;
	sampler2D texture_diffuse1;
	//sampler2D texture_diffuse2;

	bool usingSpecularMap;
	float specularComponent;
	sampler2D texture_specular1;
	//sampler2D texture_specular2;

	bool usingNormalMap;
	sampler2D texture_normal1;
	//sampler2D texture_normal2;

	float shininess;
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

uniform sampler2D shadowMaps[MaxLights];
uniform samplerCube shadowCube;

out vec4 FragColor;

uniform Material material;

uniform int lightCount;
uniform Light Lights[MaxLights];

uniform float lightNearPlane;
uniform float lightFarPlane;

in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} fs_in;

in TangentData{
	vec3 tangentDir[MaxLights];
	vec3 tangentPos[MaxLights];
} tangentData;

vec3 CalcLight(Light light, vec3 lightDir, float attenuation, float intensity, float shadow);
vec3 CalcPointLight(Light l, int index);
vec3 CalcSpotLight(Light l, int index);
vec3 CalcDirLight(Light l, int index);  
float CalcShadow(vec4 fragPosLightSpace, in sampler2D shadowMap, bool linearizeDepth);
float CalcShadowFromCube(vec3 fragPos, vec3 lightPos, in samplerCube shadowMap);
float LinearizeLightSpaceDepth(float depth);
float SampleShadowMap(in sampler2D shadowMap, vec2 texCoords, float compare, bool linearize);
float SampleShadowMapSmooth(in sampler2D shadowMap, vec2 texCoords, float compare, bool linearize, vec2 texelSize);

vec3 Diffuse;
vec3 Specular;
vec3 Normal;
vec3 ViewDir;

void main()
{
	if(material.usingNormalMap){
		Normal = texture(material.texture_normal1, fs_in.TexCoords).rgb;
		Normal = normalize(Normal * 2.0 - 1.0);
	}else{
		Normal = vec3(0, 0, 1);
	}

	if(material.usingDiffuseMap)
		Diffuse = vec3(texture(material.texture_diffuse1, fs_in.TexCoords));
	else
		Diffuse = vec3(material.diffuseColor);

	if(material.usingSpecularMap)
		Specular = vec3(texture(material.texture_specular1, fs_in.TexCoords));
	else
		Specular = vec3(material.specularComponent);

	ViewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

	vec3 Result = vec3(0.0);

	for(int i = 0; i < lightCount; ++i)
	{
		switch(Lights[i].type){
			case 0:
				Result += CalcPointLight(Lights[i], i);
				break;
			case 1: 
				Result += CalcSpotLight(Lights[i], i);
				break;
			case 2:
				Result += CalcDirLight(Lights[i], i);
				break;
		}
	}

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

	float shadow = 0.0;
	vec2 stepSize = 1.0 /textureSize(shadowMap, 0);
	vec2 pcfPos;

	for(int i=0; i<9; i++)
	{
		pcfPos = projectedFragPos.xy + (vec2(-1 + i/3, -1 + mod(i,3)) * stepSize);
		shadow += SampleShadowMapSmooth(shadowMap, pcfPos, currentDepth - .0001, linearizeDepth, stepSize);	
	}

    return shadow / 9;
}

float SampleShadowMap(in sampler2D shadowMap, vec2 texCoords, float compare, bool linearize)
{
	float depth = texture(shadowMap, texCoords).r;
	if(linearize) depth = LinearizeLightSpaceDepth(depth);
	return compare > depth ? 0.0 : 1.0;
}

//Learnt from this video: https://www.youtube.com/watch?v=yn5UJzMqxj0
float SampleShadowMapSmooth(in sampler2D shadowMap, vec2 texCoords, float compare, bool linearize, vec2 texelSize)
{
	vec2 pixelCoords = texCoords / texelSize + vec2(0.5);
	vec2 coordsFract = fract(pixelCoords);
	vec2 basePixel = (pixelCoords - coordsFract) * texelSize;	

	float blPixel = SampleShadowMap(shadowMap, basePixel, compare, linearize);
	float brPixel = SampleShadowMap(shadowMap, basePixel + vec2(texelSize.x, 0.0), compare, linearize);
	float tlPixel = SampleShadowMap(shadowMap, basePixel + vec2(0.0, texelSize.y), compare, linearize);
	float trPixel = SampleShadowMap(shadowMap, basePixel + texelSize, compare, linearize);

	return mix(mix(blPixel, tlPixel, coordsFract.y), mix(brPixel, trPixel, coordsFract.y), coordsFract.x);
}

float CalcShadowFromCube(vec3 fragPos, vec3 lightPos, in samplerCube shadowMap)
{
	vec3 fragToLight = fragPos - lightPos;
	
	float closest = texture(shadowMap, fragToLight).r * lightFarPlane;

	float bias = 0.05;
	float shadow = (length(fragToLight) - bias > closest) ? 0 : 1;

	return shadow;
}

float LinearizeLightSpaceDepth(float depth)
{
    float z = depth * 2.0 - 1.0;
    return (2.0 * lightNearPlane * lightFarPlane) / (lightFarPlane + lightNearPlane - z * (lightFarPlane - lightNearPlane));
}

vec3 CalcLight(Light light, vec3 lightDir, float attenuation, float intensity, float shadow)
{
	float diff = max(dot(Normal, lightDir), 0.0);
	float spec = pow(max(dot(Normal, normalize(lightDir + ViewDir)), 0.0), material.shininess * 3);

	vec3 ambient = light.ambient * attenuation * Diffuse;
	vec3 diffuse = light.diffuse * intensity * attenuation * diff * Diffuse;
	vec3 specular = light.specular * spec * Specular * intensity * attenuation;
	return vec3(ambient + (shadow * diffuse) + (shadow * specular));
}

vec3 CalcPointLight(Light l, int index)
{
	vec3 tangentPos = tangentData.tangentPos[index];

	float distance = length((tangentPos) - fs_in.TangentFragPos);
	float attenuation = 1 / (l.constant + (l.linear * distance) + (l.quadratic * pow(distance, 2)));
	
	float shadow = 1;

	//TODO add support for multiple shadow-enabled point lights
	if(l.shadowsEnabled)
		shadow = CalcShadowFromCube(fs_in.FragPos, l.position, shadowCube);

	return CalcLight(l, normalize(tangentPos - fs_in.TangentFragPos), attenuation, 1.0, shadow);
}

vec3 CalcSpotLight(Light l, int index)
{
	vec3 tangentPos = tangentData.tangentPos[index];

	float distance = length(tangentPos - fs_in.TangentFragPos);
	float attenuation = 1 / (l.constant + (l.linear * distance) + (l.quadratic * pow(distance, 2)));

	vec3 lightDir = normalize(tangentPos - fs_in.TangentFragPos);
	float theta = dot(lightDir, normalize(-(tangentData.tangentDir[index])));
	float epsilon = l.cutoff - l.outerCutoff;
	float intensity = clamp((theta - l.outerCutoff) / epsilon, 0.0, 1.0);

	float shadow = 1;

	if(l.shadowsEnabled && intensity > 0) 
		shadow = CalcShadow(l.lightSpace * vec4(fs_in.FragPos, 1.0), shadowMaps[l.shadowIndex], true);

	return CalcLight(l, lightDir, attenuation, intensity, shadow);
}

vec3 CalcDirLight(Light l, int index)
{
	float shadow = 1;

	if(l.shadowsEnabled)
		shadow = CalcShadow(l.lightSpace * vec4(fs_in.FragPos, 1.0), shadowMaps[l.shadowIndex], false);

	return CalcLight(l, normalize(-(tangentData.tangentDir[index])), 1.0, 1.0, shadow);
}