/*  by Javi Agenjo 2013 UPF  javi.agenjo@gmail.com
	This class encapsulates the game, is in charge of creating the game, getting the user input, process the update and render.
*/

#ifndef APPLICATION_H
#define APPLICATION_H

#include "includes.h"
#include "camera.h"
#include "utils.h"
#include "scenenode.h"


class Application
{
public:
	static Application* instance;

	std::vector <SceneNode*> node_list;

	// window
	SDL_Window* window;
	int window_width;
	int window_height;

	// some globals
	long frame;
	float time;
	float elapsed_time;
	int fps;
	bool must_exit;
	bool render_debug;
	float step_length;
	float brightness;
	Vector4 plane;
	float iso_threshold;
	float h_value;

	bool jittering;
	bool TF;
	bool VC;
	bool isosurface;

	Texture* LUTtexture = NULL;

	enum eVolums {
		CTABDOMEN,
		ORANGE,
		TEA
	};

	enum eJittering {
		METODO1,
		METODO2
	};

	eVolums volums;
	eJittering jitteringm;

	// some vars
	static Camera* camera; //our GLOBAL camera
	bool mouse_locked; //tells if the mouse is locked (not seen)

	// constructor
	Application(int window_width, int window_height, SDL_Window* window);

	// main functions
	void render(void);
	void update(double dt);

	// events
	void onKeyDown(SDL_KeyboardEvent event);
	void onKeyUp(SDL_KeyboardEvent event);
	void onMouseButtonDown(SDL_MouseButtonEvent event);
	void onMouseButtonUp(SDL_MouseButtonEvent event);
	void onMouseWheel(SDL_MouseWheelEvent event);
	void onGamepadButtonDown(SDL_JoyButtonEvent event);
	void onGamepadButtonUp(SDL_JoyButtonEvent event);
	void onResize(int width, int height);
	void onFileChanged(const char* filename);

	void renderInMenu();
};


#endif 