/* 
THIS IS AUTOGENERATED CODE :D

==============================================================

Had some driver issues which appeared when compiling
function definitions on GLES 3 and my machine does not
even support Vulkan so...

If you're curious about the shader check the other similarly
named file with the .brokenshader suffix, if you're interested
about the code generation check out the srcgen directory. It's
a lazy implementation though because game jam but yeah..

Good luck!

==============================================================
*/

/*
Some useful predefined stuff:
      textureLod(SCREEN_TEXTURE, *some uv*, 0.0)  >> input: vec4
      SCREEN_UV  >> input: vec2
      SCREEN_TEXTURE  >> input: vec2?
      COLOR  >> output: vec3
      SCREEN_PIXEL_SIZE  >> input: vec2
*/   

shader_type canvas_item;

// Default: 32.0, 28.0
uniform vec2 size = vec2(16.0, 14.0);
uniform vec4 my_color1 = vec4(1,1,1,1);
uniform vec4 my_color2 = vec4(1,1,1,1);
uniform vec4 my_color3 = vec4(1,1,1,1);
uniform vec4 my_color4 = vec4(1,1,1,1);

uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

// Function definitions are broken in this version,
// so we'll just generate the code so it's inline :(


// NOTE(srp): holy grail inputs: SCREEN_TEXTURE, DEPTH_TEXTURE
// NOTE(srp): Transparent objects won't be caught in the SCREEN_TEXTURE
void fragment() {
    vec3 c = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;



    // Parameters
    vec3 __gen_rgb2hsv1_c = my_color1.rgb;

    // Function Body
    vec4 __gen_rgb2hsv1_K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 __gen_rgb2hsv1_p = mix(vec4(__gen_rgb2hsv1_c.bg, __gen_rgb2hsv1_K.wz), vec4(__gen_rgb2hsv1_c.gb, __gen_rgb2hsv1_K.xy), step(__gen_rgb2hsv1_c.b, __gen_rgb2hsv1_c.g));
    vec4 __gen_rgb2hsv1_q = mix(vec4(__gen_rgb2hsv1_p.xyw, __gen_rgb2hsv1_c.r), vec4(c.r, __gen_rgb2hsv1_p.yzx), step(__gen_rgb2hsv1_p.x, __gen_rgb2hsv1_c.r));

    float __gen_rgb2hsv1_d = __gen_rgb2hsv1_q.x - min(__gen_rgb2hsv1_q.w, __gen_rgb2hsv1_q.y);
    float __gen_rgb2hsv1_e = 1.0e-10;
    


    vec3 col1hsv =  vec3(abs(__gen_rgb2hsv1_q.z + (__gen_rgb2hsv1_q.w - __gen_rgb2hsv1_q.y) / (6.0 * __gen_rgb2hsv1_d + __gen_rgb2hsv1_e)), __gen_rgb2hsv1_d / (__gen_rgb2hsv1_q.x + __gen_rgb2hsv1_e), __gen_rgb2hsv1_q.x) ;
    
    // Brightness, contrast, saturation
    c.rgb = mix(vec3(0.0), c.rgb, brightness);
    c.rgb = mix(vec3(0.5), c.rgb, contrast);
    c.rgb = mix(vec3(dot(vec3(1.0), c.rgb) * 0.33333), c.rgb, saturation);

    COLOR.rgb = c;
}
