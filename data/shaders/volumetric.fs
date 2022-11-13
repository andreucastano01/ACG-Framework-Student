uniform vec3 u_camera_position;
uniform sampler3D u_texture;
uniform float ray_step;
uniform float brightness;
uniform mat4 u_inverse_model;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const int SAMPLES = 1024;

float random (vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

//Mirar slide coordinate systems mucho, parece que al crear variables no esta todo en las mismas coordenadas,
//y el sample_pos tambien tenemos que cambiar coordenadas, anadir el otro early termination
void main()
{
    vec4 world_position = u_inverse_model * vec4(v_world_position, 1.0);
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
    //ray_start += random(v_uv);
	vec3 ray_dir = (world_position.xyz - ray_start);
    ray_dir = normalize(ray_dir);
    //vec3 sample_pos = clamp(v_position, 0.0, 1.0);
    vec3 sample_pos = v_position;
    vec3 samplepos01 = sample_pos * 0.5 + 0.5;
    vec3 stepLength = ray_dir * ray_step;

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        float d = texture(u_texture, samplepos01).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);
        //Calculamos color
        final_color += stepLength * (1.0 - final_color.a) * sample_color;
        //Pasamos al siguiente sample
        samplepos01 += stepLength;
        //Early termination
        if(final_color.a >= 1) break;
        if(samplepos01.x > 1 || samplepos01.x < 0 || samplepos01.y > 1 || samplepos01.y < 0 || samplepos01.z > 1 || samplepos01.z < 0) break;
    }
    //Anades brightness
    //if (final_color.x <= 0.1 || final_color.y <= 0.1 || final_color.z <= 0.1) discard;
	gl_FragColor = final_color * brightness;
}