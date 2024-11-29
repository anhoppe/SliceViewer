#version 140
#extension GL_ARB_explicit_attrib_location : enable

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 color;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    color = vertexColor;
}
