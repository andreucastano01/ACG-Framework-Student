#include "application.h"
#include "utils.h"
#include "mesh.h"
#include "texture.h"
#include "volume.h"
#include "fbo.h"
#include "shader.h"
#include "input.h"
#include "animation.h"
#include "extra/hdre.h"
#include "extra/imgui/imgui.h"
#include "extra/imgui/imgui_impl_sdl.h"
#include "extra/imgui/imgui_impl_opengl3.h"
#include "material.h"

#include <cmath>

bool render_wireframe = false;
Camera* Application::camera = nullptr;
Application* Application::instance = NULL;

Application::Application(int window_width, int window_height, SDL_Window* window)
{
	this->window_width = window_width;
	this->window_height = window_height;
	this->window = window;
	instance = this;
	must_exit = false;
	render_debug = true;
	volums = CTABDOMEN;
	step_length = 0.01f;
	brightness = 4.65f;
	plane = Vector4(1, 1, 1, 1);

	fps = 0;
	frame = 0;
	time = 0.0f;
	elapsed_time = 0.0f;
	mouse_locked = false;

	// OpenGL flags
	glEnable( GL_CULL_FACE ); //render both sides of every triangle
	glEnable( GL_DEPTH_TEST ); //check the occlusions using the Z buffer

	// Create camera
	camera = new Camera();
	camera->lookAt(Vector3(5.f, 5.f, 5.f), Vector3(0.f, 0.0f, 0.f), Vector3(0.f, 1.f, 0.f));
	camera->setPerspective(45.f, window_width/(float)window_height, 0.1f, 10000.f); //set the projection, we want to be perspective

	// EXAMPLE OF HOW TO CREATE A SCENE NODE
	SceneNode* node = new SceneNode("Visible node");
	node->mesh = Mesh::Get("data/meshes/sphere.obj.mbin");
	node->model.scale(1, 1, 1);
	StandardMaterial* mat = new StandardMaterial();
	node->material = mat;
	mat->shader = Shader::Get("data/shaders/basic.vs", "data/shaders/normal.fs");
	//node_list.push_back(node);

	// TODO: create all the volumes to use in the app
	//new VolumeNode(autoset a cube for the mesh of the class)
	SceneNode* abdomennode = new SceneNode("CTABDOMEN");
	Mesh* mesh = new Mesh();
	mesh->createCube();
	abdomennode->mesh = mesh;
	//load Volume from dataset
	Volume* volume = new Volume();
	volume->loadPVM("data/volumes/CT-Abdomen.pvm");
	//create Texture from Value
	Texture* texture = new Texture();
	texture->create3DFromVolume(volume, GL_REPEAT);
	//create Material from Texture
	VolumeMaterial* volumematerial = new VolumeMaterial();
	volumematerial->texture = texture;
	//set material of  the VolumeNode as the material created
	abdomennode->material = volumematerial;
	//check that this is created node is used in the main render call
	node_list.push_back(abdomennode);
	
	SceneNode* orangenode = new SceneNode("Orange");
	orangenode->mesh = mesh;
	//load Volume from dataset
	Volume* orangevolume = new Volume();
	orangevolume->loadPVM("data/volumes/Orange.pvm");
	//create Texture from Value
	Texture* orangetexture = new Texture();
	orangetexture->create3DFromVolume(orangevolume, GL_REPEAT);
	//create Material from Texture
	VolumeMaterial* orangevolumematerial = new VolumeMaterial();
	orangevolumematerial->texture = orangetexture;
	//set material of  the VolumeNode as the material created
	orangenode->material = orangevolumematerial;
	//check that this is created node is used in the main render call
	node_list.push_back(orangenode);

	SceneNode* teanode = new SceneNode("Tea");
	teanode->mesh = mesh;
	//load Volume from dataset
	Volume* teavolume = new Volume();
	teavolume->loadPNG("data/volumes/teapot_16_16.png");
	//create Texture from Value
	Texture* teatexture = new Texture();
	teatexture->create3DFromVolume(teavolume, GL_REPEAT);
	//create Material from Texture
	VolumeMaterial* teavolumematerial = new VolumeMaterial();
	teavolumematerial->texture = teatexture;
	//set material of  the VolumeNode as the material created
	teanode->material = teavolumematerial;
	//check that this is created node is used in the main render call
	node_list.push_back(teanode);
	
	//LUTTexture
	LUTtexture = new Texture(window_width, 1, GL_RGB, GL_FLOAT, false);
	LUTtexture->load("data/images/aaa.png");
	//hide the cursor
	SDL_ShowCursor(!mouse_locked); //hide or show the mouse
}

// what to do when the image has to be draw
void Application::render(void)
{
	// set the clear color (the background color)
	glClearColor(0.1, 0.1, 0.1, 1.0);

	// Clear the window and the depth buffer
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	// set the camera as default
	camera->enable();

	// set flags
	glEnable(GL_DEPTH_TEST);
	glDisable(GL_CULL_FACE);

	for (size_t i = 0; i < node_list.size(); i++) {
		if (volums == i) {
			node_list[i]->render(camera);
		}

		if(render_wireframe)
			node_list[i]->renderWireframe(camera);
	}

	//Draw the floor grid
	if(render_debug)
		drawGrid();
}

void Application::update(double seconds_elapsed)
{
	float speed = seconds_elapsed * 10; //the speed is defined by the seconds_elapsed so it goes constant
	float orbit_speed = seconds_elapsed * 1;

	// example
	float angle = seconds_elapsed * 10.f * DEG2RAD;
	/*for (int i = 0; i < root.size(); i++) {
		root[i]->model.rotate(angle, Vector3(0,1,0));
	}*/

	// mouse input to rotate the cam
	if ((Input::mouse_state & SDL_BUTTON_LEFT && !ImGui::IsAnyWindowHovered()
		&& !ImGui::IsAnyItemHovered() && !ImGui::IsAnyItemActive())) //is left button pressed?
	{
		camera->orbit(-Input::mouse_delta.x * orbit_speed, Input::mouse_delta.y * orbit_speed);
	}

	// async input to move the camera around
	if (Input::isKeyPressed(SDL_SCANCODE_LSHIFT)) speed *= 10; //move faster with left shift
	if (Input::isKeyPressed(SDL_SCANCODE_W) || Input::isKeyPressed(SDL_SCANCODE_UP)) camera->move(Vector3(0.0f, 0.0f, 1.0f) * speed);
	if (Input::isKeyPressed(SDL_SCANCODE_S) || Input::isKeyPressed(SDL_SCANCODE_DOWN)) camera->move(Vector3(0.0f, 0.0f, -1.0f) * speed);
	if (Input::isKeyPressed(SDL_SCANCODE_A) || Input::isKeyPressed(SDL_SCANCODE_LEFT)) camera->move(Vector3(1.0f, 0.0f, 0.0f) * speed);
	if (Input::isKeyPressed(SDL_SCANCODE_D) || Input::isKeyPressed(SDL_SCANCODE_RIGHT)) camera->move(Vector3(-1.0f, 0.0f, 0.0f) * speed);
	if (Input::isKeyPressed(SDL_SCANCODE_SPACE)) camera->moveGlobal(Vector3(0.0f, -1.0f, 0.0f) * speed);
	if (Input::isKeyPressed(SDL_SCANCODE_LCTRL)) camera->moveGlobal(Vector3(0.0f, 1.0f, 0.0f) * speed);

	// to navigate with the mouse fixed in the middle
	if (mouse_locked)
		Input::centerMouse();
}

// Keyboard event handler (sync input)
void Application::onKeyDown(SDL_KeyboardEvent event)
{
	switch (event.keysym.sym)
	{
	case SDLK_ESCAPE: must_exit = true; break; //ESC key, kill the app
	case SDLK_F1: render_debug = !render_debug; break;
	case SDLK_F2: render_wireframe = !render_wireframe; break;
	case SDLK_F5: Shader::ReloadAll(); break;
	}
}

void Application::onKeyUp(SDL_KeyboardEvent event)
{
}

void Application::onGamepadButtonDown(SDL_JoyButtonEvent event)
{

}

void Application::onGamepadButtonUp(SDL_JoyButtonEvent event)
{

}

void Application::onMouseButtonDown(SDL_MouseButtonEvent event)
{
	if (event.button == SDL_BUTTON_MIDDLE) //middle mouse
	{
		mouse_locked = !mouse_locked;
		SDL_ShowCursor(!mouse_locked);
	}
}

void Application::onMouseButtonUp(SDL_MouseButtonEvent event)
{
}

void Application::onMouseWheel(SDL_MouseWheelEvent event)
{
	ImGuiIO& io = ImGui::GetIO();
	switch (event.type)
	{
	case SDL_MOUSEWHEEL:
	{
		if (event.x > 0) io.MouseWheelH += 1;
		if (event.x < 0) io.MouseWheelH -= 1;
		if (event.y > 0) io.MouseWheel += 1;
		if (event.y < 0) io.MouseWheel -= 1;
	}
	}

	if (!ImGui::IsAnyWindowHovered() && event.y)
		camera->changeDistance(event.y * 0.5);
}

void Application::onResize(int width, int height)
{
	std::cout << "window resized: " << width << "," << height << std::endl;
	glViewport(0, 0, width, height);
	camera->aspect = width / (float)height;
	window_width = width;
	window_height = height;
}

void Application::onFileChanged(const char* filename)
{
	Shader::ReloadAll();
}

void Application::renderInMenu() {

	if (ImGui::TreeNode("Scene")) {
		//
		ImGui::TreePop();
	}

	if (ImGui::TreeNode("Camera")) {
		camera->renderInMenu();
		ImGui::TreePop();
	}

	//Scene graph
	if (ImGui::TreeNode("Entities"))
	{
		unsigned int count = 0;
		std::stringstream ss;
		for (auto& node : node_list)
		{
			ss << count;
			if (ImGui::TreeNode(node->name.c_str()))
			{
				node->renderInMenu();
				ImGui::TreePop();
			}
			++count;
			ss.str("");
		}
		ImGui::TreePop();
	}

	ImGui::Checkbox("Render debug", &render_debug);
	ImGui::Checkbox("Wireframe", &render_wireframe);
	ImGui::Combo("Volums", (int*)&volums, "CTABDOMEN\0ORANGE\0TEA", 2);
	ImGui::SliderFloat("Step Length", &step_length, 0.01f, 0.2f);
	ImGui::SliderFloat("Brightness", &brightness, 0.01f, 5.f);
	ImGui::DragFloat4("plane", (float*)&plane);
}
