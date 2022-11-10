uniform vec3 u_camera_position;
uniform sampler3D u_texture;
uniform float ray_step;
uniform float brightness;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const int SAMPLES = 64;

//Mirar slide coordinate systems mucho, parece que al crear variables no esta todo en las mismas coordenadas,
//y el sample_pos tambien tenemos que cambiar coordenadas, anadir el otro early termination
void main()
{
    //v_world_position <-- 
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
	vec3 ray_dir = (v_world_position - ray_start); //<--
    ray_dir = normalize(ray_dir);
    vec3 sample_pos = v_position; //Primer sample <--
    vec3 stepLength = ray_dir * ray_step;

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        float d = texture(u_texture, sample_pos).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);
        //Calculamos color
        final_color += stepLength * (1.0 - final_color.a) * sample_color;
        //Pasamos al siguiente sample
        sample_pos += stepLength;
        //Early termination
        if(final_color.a >= 1) break; //<--
    }
    //Anades brightness
	gl_FragColor = final_color * brightness;
}