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
global uniform vec4 my_color1;
global uniform vec4 my_color2;
global uniform vec4 my_color3;
global uniform vec4 my_color4;

uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

// Function definitions are broken in this version,
// so we'll just generate the code so it's inline :(
¿¿rgb2hsv¿
    // Parameters
    vec3 $c$ = °0;

    // Function Body
    vec4 $K$ = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 $p$ = mix(vec4($c$.bg, $K$.wz), vec4($c$.gb, $K$.xy), step($c$.b, $c$.g));
    vec4 $q$ = mix(vec4($p$.xyw, $c$.r), vec4(c.r, $p$.yzx), step($p$.x, $c$.r));

    float $d$ = $q$.x - min($q$.w, $q$.y);
    float $e$ = 1.0e-10;
    >>>> vec3(abs($q$.z + ($q$.w - $q$.y) / (6.0 * $d$ + $e$)), $d$ / ($q$.x + $e$), $q$.x) >>>>
¿¿

// NOTE(srp): holy grail inputs: SCREEN_TEXTURE, DEPTH_TEXTURE
// NOTE(srp): Transparent objects won't be caught in the SCREEN_TEXTURE
void fragment() {
    vec3 c = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;

    vec3 col1hsv = ¡¡rgb2hsv(my_color1.rgb)¡¡;
    
    // Brightness, contrast, saturation
    c.rgb = mix(vec3(0.0), c.rgb, brightness);
    c.rgb = mix(vec3(0.5), c.rgb, contrast);
    c.rgb = mix(vec3(dot(vec3(1.0), c.rgb) * 0.33333), c.rgb, saturation);

    COLOR.rgb = c;
}
