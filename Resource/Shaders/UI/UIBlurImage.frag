#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;
uniform sampler2D image;
uniform vec2 screenSize;
uniform vec4 tint = vec4(1);



void main()
{
	vec2 screenPos = gl_FragCoord.xy / screenSize;
	vec2 pixelSize = vec2(1.0, 1.0) / screenSize;

	vec3 color = vec3(0);

	for(int i = 0; i < 9; i++)
	{
		for(int j = 0; j < 9; j++)
		{
			color += texture(screen, screenPos + (vec2(i-4, j-4) * pixelSize)).xyz / 81.0;
		}
	}

	vec4 texColor = texture2D(image, TexCoords);
	FragColor = (texColor + ((1.0 - texColor.a) * vec4(color, 1.0))) * tint;
}