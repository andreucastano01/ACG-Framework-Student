uniform vec3 u_camera_position;
uniform sampler2D u_texture;
uniform vec3 ray_step;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const int SAMPLES = 64;

void main()
{
    vec3 ray_start = u_camera_position;
    vec3 sample_pos = vec3(0.0); //Cambiar
	vec3 ray_dir = (v_world_position - u_camera_position);
    float d = texture(u_texture, v_uv).x;
    vec4 final_color = vec4(0.0);

    for(int i = 0; i < SAMPLES; i++){
        vec4 sample_color = vec4(d,1-d,0,d*d);
        final_color += ray_step * (1.0 - final_color.a) * sample_color;
        sample_pos += ray_step;
    }

	gl_FragColor = vec4(v_color);
}