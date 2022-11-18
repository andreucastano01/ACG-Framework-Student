uniform vec3 u_camera_position;
uniform sampler3D u_texture;
uniform sampler1D LUT_texture;
uniform float ray_step;
uniform float brightness;
uniform mat4 u_inverse_model;
uniform float texture_width;
uniform vec4 u_plane;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const int SAMPLES = 1024;

float random (vec2 co) {
    return fract(sin(dot(co.xy, vec2(12.9898,78.233)))* 43758.5453123);
}

float noise (in vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return mix(a, b, u.x) +
            (c - a)* u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
}

//Mirar slide coordinate systems mucho, parece que al crear variables no esta todo en las mismas coordenadas,
//y el sample_pos tambien tenemos que cambiar coordenadas, anadir el otro early termination
void main()
{
    vec4 world_position = u_inverse_model * vec4(v_world_position, 1.0);
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
    vec3 sample_pos = v_position;
    vec3 samplepos01 = sample_pos * 0.5 + 0.5;
    
    //Jittering
    //ray_start += noise(v_uv);
    vec2 offset = v_uv / texture_width;
    ray_start += offset;

	vec3 ray_dir = (world_position.xyz - ray_start);
    ray_dir = normalize(ray_dir);
    vec3 stepLength = ray_dir * ray_step;

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        float d = texture(u_texture, samplepos01).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);

        //Calculamos color  
        if(u_plane.x*samplepos01.x + u_plane.y*samplepos01.y + u_plane.z*samplepos01.z + u_plane.w > 0)
            final_color += stepLength * (1.0 - final_color.a) * sample_color;

        //Intento de transfer function
        //vec3 c = texture(LUT_texture, final_color.a).xyz;
        //vec4 color = vec4(c, final_color.a);
        //final_color = color;

        //Pasamos al siguiente sample
        samplepos01 += stepLength;
        //Early termination
        if(final_color.a >= 1) break;
        if(samplepos01.x > 1 || samplepos01.x < 0 || samplepos01.y > 1 || samplepos01.y < 0 || samplepos01.z > 1 || samplepos01.z < 0) break;
    }
    //Anades brightness
	gl_FragColor = final_color * brightness;
}