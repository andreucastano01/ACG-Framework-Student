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

void main()
{
    //Creamos variables y el rayo
    vec3 ray_start = u_camera_position;
	vec3 ray_dir = (v_world_position - ray_start);
    ray_dir = normalize(ray_dir);

    vec3 sample_pos = vec3(v_position);
    vec4 final_color = vec4(0.0);

    for(int i = 0; i < SAMPLES; i++){
        float stepLength = i * ray_step;
        sample_pos += stepLength;
        float d = texture(u_texture, sample_pos).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);
        final_color += stepLength * (1.0 - final_color.a) * sample_color;
        if(final_color.a >= 1) break;
    }

	gl_FragColor = vec4(final_color) * brightness;
}