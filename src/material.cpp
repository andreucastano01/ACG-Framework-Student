#include "material.h"
#include "texture.h"
#include "application.h"
#include "extra/hdre.h"

StandardMaterial::StandardMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

StandardMaterial::~StandardMaterial()
{

}

void StandardMaterial::setUniforms(Camera* camera, Matrix44 model)
{
	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", camera->eye);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_color", color);

	if (texture)
		shader->setUniform("u_texture", texture);
}

void StandardMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (mesh && shader)
	{
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
	}
}

void StandardMaterial::renderInMenu()
{
	ImGui::ColorEdit3("Color", (float*)&color); // Edit 3 floats representing a color
}

VolumeMaterial::VolumeMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/volumetric.fs");
}	

VolumeMaterial::~VolumeMaterial()
{

}

void VolumeMaterial::setUniforms(Camera* camera, Matrix44 model)
{
	Matrix44 aux_model;
	aux_model = model;
	aux_model.inverse();
	vec3 cameye = aux_model * camera->eye;
	//upload node uniforms
	shader->setUniform("u_viewprojection", camera->viewprojection_matrix);
	shader->setUniform("u_camera_position", cameye);
	shader->setUniform("u_inverse_model", aux_model);
	shader->setUniform("u_model", model);
	shader->setUniform("u_time", Application::instance->time);
	shader->setUniform("u_color", color);
	shader->setUniform("ray_step", Application::instance->step_length);
	shader->setUniform("brightness", Application::instance->brightness);
	if (texture) {
		shader->setUniform("u_texture", texture, 1);
		shader->setUniform("texture_width", texture->width);
	}
	if (Application::instance->LUTtexture)
		shader->setUniform("LUT_texture", Application::instance->LUTtexture, 2);
	if (Application::instance->noisetexture)
		shader->setUniform("noise_texture", Application::instance->LUTtexture, 3);
	shader->setUniform("u_plane", Application::instance->plane);
	
	if (!Application::instance->jittering) shader->setUniform1("u_have_jittering", 0);
	else {
		shader->setUniform1("u_have_jittering", 1);
		if(Application::instance->jitteringm == 1) shader->setUniform1("u_have_jittering_2", 1);
		else shader->setUniform1("u_have_jittering_2", 0);
	}

	if (Application::instance->VC) shader->setUniform1("u_have_vc", 1);
	else shader->setUniform1("u_have_vc", 0);

	if (Application::instance->TF) shader->setUniform1("u_have_tf", 1);
	else shader->setUniform1("u_have_tf", 0);
}

void VolumeMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	glEnable(GL_CULL_FACE);
	glCullFace(GL_BACK);
	glEnable(GL_BLEND);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	if (Application::instance->isosurface) shader = Shader::Get("data/shaders/basic.vs", "data/shaders/isosurface.fs");
	else shader = Shader::Get("data/shaders/basic.vs", "data/shaders/volumetric.fs");
	if (mesh && shader)
	{
		//enable shader
		shader->enable();

		//upload uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		//disable shader
		shader->disable();
	}
}

void VolumeMaterial::renderInMenu()
{

}

WireframeMaterial::WireframeMaterial()
{
	color = vec4(1.f, 1.f, 1.f, 1.f);
	shader = Shader::Get("data/shaders/basic.vs", "data/shaders/flat.fs");
}

WireframeMaterial::~WireframeMaterial()
{

}

void WireframeMaterial::render(Mesh* mesh, Matrix44 model, Camera* camera)
{
	if (shader && mesh)
	{
		glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);

		//enable shader
		shader->enable();

		//upload material specific uniforms
		setUniforms(camera, model);

		//do the draw call
		mesh->render(GL_TRIANGLES);

		glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
	}
}