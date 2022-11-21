uniform vec3 u_camera_position;
uniform sampler3D u_texture;
uniform sampler2D LUT_texture;
uniform sampler2D noise_texture;
uniform float ray_step;
uniform float brightness;
uniform mat4 u_inverse_model;
uniform float texture_width;
uniform vec4 u_plane;
uniform int u_have_jittering;
uniform int u_have_jittering_met;
uniform int u_have_vc;
uniform int u_have_tf;

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
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
    vec3 sample_pos = v_position;
    vec3 ray_dir = (sample_pos - ray_start);
    ray_dir = normalize(ray_dir);
    float offset = 1.0;

    //Jittering
    if(u_have_jittering == 1){
        if(u_have_jittering_met == 1){
            vec2 uv_noise = gl_FragCoord.xy / texture_width;
            uv_noise = uv_noise * 0.5 + 0.5;
            offset = texture(noise_texture, uv_noise).x;
        } else{
            offset = noise(v_uv);
        }
    }
    
    vec3 stepLength = ray_dir * ray_step * offset; //Mirar stepLength
    vec2 uv_LUT = vec2(1.0);

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        vec3 samplepos01 = sample_pos * 0.5 + 0.5;
        float d = texture(u_texture, samplepos01).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);

        //Calculamos color
        if (u_have_vc == 1){
            if(u_plane.x*sample_pos.x + u_plane.y*sample_pos.y + u_plane.z*sample_pos.z + u_plane.w < 0)
                final_color += stepLength * (1.0 - final_color.a) * sample_color;
        }
        else final_color += stepLength * (1.0 - final_color.a) * sample_color;

        //Intento de transfer function
        if (u_have_tf == 1){
            uv_LUT.x = final_color.a;
            uv_LUT = uv_LUT * 0.5 + 0.5;
            vec3 c = texture(LUT_texture, uv_LUT).xyz;
            final_color.xyz = final_color.xyz * c;
        }

        //Pasamos al siguiente sample
        sample_pos += stepLength;

        //Early termination
        if(final_color.a >= 1) break;
        if(sample_pos.x > 1 || sample_pos.x < -1 || sample_pos.y > 1 || sample_pos.y < -1 || sample_pos.z > 1 || sample_pos.z < -1) break;
    }
    //Anades brightness
	gl_FragColor = final_color * brightness;
}