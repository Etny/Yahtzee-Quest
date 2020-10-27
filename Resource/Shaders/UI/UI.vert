#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexCoords;

out vec2 TexCoords;

uniform mat4 model = mat4(1.0, 0.0, 0.0, 0.0,   
                          0.0, 1.0, 0.0, 0.0,
                          0.0, 0.0, 1.0, 0.0,
						  0.0, 0.0, 0.0, 1.0);

uniform vec2 screenSize = vec2(1920, 1080);

uniform float depth = 0;

void main()
{
	vec2 pixelPos = (model * vec4(aPos, 0, 1)).xy;
	gl_Position = vec4(pixelPos / screenSize, depth, 1);
	TexCoords = aTexCoords;
}