uniform vec3 u_camera_position;
uniform sampler3D u_texture;
uniform sampler1D LUT_texture;
uniform float ray_step;
uniform float brightness;
uniform mat4 u_inverse_model;
uniform float texture_width;
uniform vec4 u_plane;
uniform float iso_threshold;
uniform float h_value;
uniform vec3 l_position;

varying vec3 v_position;
varying vec3 v_world_position;
varying vec3 v_normal;
varying vec2 v_uv;
varying vec4 v_color;

const int SAMPLES = 1024;

void main()
{
    //Creamos variables y el rayo
    vec4 final_color = vec4(0.0);
    vec3 ray_start = u_camera_position;
    vec3 sample_pos = v_position;
    
	vec3 ray_dir = (sample_pos - ray_start);
    ray_dir = normalize(ray_dir);
    vec3 stepLength = ray_dir * ray_step;

    //Sampleamos cada punto
    for(int i = 0; i < SAMPLES; i++){
        vec3 samplepos01 = sample_pos * 0.5 + 0.5;
        float d = texture(u_texture, samplepos01).x;
        vec4 sample_color = vec4(d,1-d,0,d*d);

        //Calculamos color  
        if(sample_color.w >= iso_threshold){
            float x1 = texture(u_texture, vec3(samplepos01.x + h_value, samplepos01.yz)).x;
            float x2 = texture(u_texture, vec3(samplepos01.x - h_value, samplepos01.yz)).x;
            float y1 = texture(u_texture, vec3(samplepos01.x, samplepos01.y + h_value, samplepos01.z)).x;
            float y2 = texture(u_texture, vec3(samplepos01.x, samplepos01.y - h_value, samplepos01.z)).x;
            float z1 = texture(u_texture, vec3(samplepos01.xy, samplepos01.z + h_value)).x;
            float z2 = texture(u_texture, vec3(samplepos01.xy, samplepos01.z - h_value)).x;
            vec3 N = (1/(2*h_value))*vec3(x1 - x2, y1 - y2, z1 - z2);
            
            //Phong
            N = normalize(-N);
            vec3 L = sample_pos - l_position;
            L = normalize(L);
            vec3 V = sample_pos - u_camera_position;
            V = normalize(V);
            vec3 R = 2*(N*L)*N - L;
            float a = pow((R.x * V.x + R.y * V.y + R.z * V.z), 10.0);
            final_color = vec4(sample_color.xyz * (L*N + a), 1.0);
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