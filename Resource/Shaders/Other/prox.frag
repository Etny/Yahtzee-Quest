#version 330 core

out vec4 FragColor;

uniform vec3 Color = vec3(1.0);
uniform int PointCount = 5;
uniform vec3 Points[5];
uniform float Threshold = 2;
uniform float AlphaCoof = 1;

in VS_OUT {
	vec3 FragPos;
	vec2 TexCoords;
	vec3 TangentFragPos;
	vec3 TangentViewPos;
	mat3 TBN;
} fs_in;

void main()
{
	float a = 1;
	float minDist = 100;

	for(int i = 0; i < PointCount; i++)
	{
		vec3 p = Points[i];
		float dist = distance(p, fs_in.FragPos);

		if(dist < minDist)
			minDist = dist;
	}

	if(minDist > Threshold) 
	{
		 a = 0;
	}
	else
	{
		a = clamp(((1.0 - (minDist / Threshold)) * AlphaCoof), 0, 1);
	}
	
	FragColor = vec4(Color, a);
}