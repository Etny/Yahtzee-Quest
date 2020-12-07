#version 330 core

in vec2 TexCoords;
out vec4 FragColor;

uniform sampler2D screen;
uniform sampler2D oldScreen;
uniform float progress;


void main()
{
	if(distance(TexCoords, vec2(0, 0)) <= .1f){
		FragColor = texture(screen, TexCoords);
	}else{
		FragColor = texture(oldScreen, TexCoords);
	}

		//FragColor = texture(screen, TexCoords);

}