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
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
    vec3 sample_pos = v_position;
    
	vec3 ray_dir = (sample_pos - ray_start);
    ray_dir = normalize(ray_dir);
    vec3 stepLength = ray_dir * ray_step;
    float h = 0.001;

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        vec3 samplepos01 = sample_pos * 0.5 + 0.5;
        float d = texture(u_texture, samplepos01).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);

        //Calculamos color  
        if(sample_color.w >= 0.07){
            final_color += stepLength * (1.0 - final_color.a) * sample_color;
            float x1 = texture(u_texture, vec3(samplepos01.x + h, samplepos01.yz)).x;
            float x2 = texture(u_texture, vec3(samplepos01.x - h, samplepos01.yz)).x;
            float y1 = texture(u_texture, vec3(samplepos01.x, samplepos01.y + h, samplepos01.z)).x;
            float y2 = texture(u_texture, vec3(samplepos01.x, samplepos01.y - h, samplepos01.z)).x;
            float z1 = texture(u_texture, vec3(samplepos01.xy, samplepos01.z + h)).x;
            float z2 = texture(u_texture, vec3(samplepos01.xy, samplepos01.z - h)).x;
            vec3 N = (1/(2*h))*vec3(x1 - x2, y1 - y2, z1 - z2);
            final_color.xyz = -N;
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