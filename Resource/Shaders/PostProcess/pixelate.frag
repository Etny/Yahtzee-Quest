#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;

vec2 pixelCount = vec2(80, 60);

void main()
{
	float pixelWidth =  (800 / pixelCount.x) * 1 / 800;
	float pixelHeight =  (600 / pixelCount.y) * 1 / 600;

	vec2 pixelOrigin = TexCoords - mod(TexCoords, vec2(pixelWidth, pixelHeight));
	vec2 pixelCorner = pixelOrigin + vec2(pixelWidth - 1/ 800, pixelHeight - 1 / 600);

	FragColor = mix(texture(screen, pixelOrigin), texture(screen, pixelCorner), 1);
}