#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;

void main()
{
	FragColor = texture(screen, TexCoords);
	if(gl_FragCoord.x > 500)
		FragColor = FragColor * vec4(.5, 1, 0.5, 1);

}