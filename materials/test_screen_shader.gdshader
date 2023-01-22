/*
Some useful predefined stuff:
      textureLod(SCREEN_TEXTURE, *some uv*, 0.0)  >> input: vec4
      SCREEN_UV  >> input: vec2
      SCREEN_TEXTURE  >> input: vec2?
      COLOR  >> output: vec3
      SCREEN_PIXEL_SIZE  >> input: vec2
*/   

shader_type canvas_item;

uniform vec3 my_color1 = vec3(0.6,0.87,0.79);
uniform vec3 my_color2 = vec3(0.84,0.93,0.76);
uniform vec3 my_color3 = vec3(1.0,0.83,0.71);
uniform vec3 my_color4 = vec3(1.0,0.67,0.65);

uniform float brightness = 1.0;
uniform float contrast = 1.0;
uniform float saturation = 1.0;

vec3 nearestHue(vec3 color, vec3 opt1, vec3 opt2, vec3 opt3, vec3 opt4)
{
    float dh1 = abs(color.z - opt1.z);
    float dh2 = abs(color.z - opt2.z);
    float dh3 = abs(color.z - opt3.z);
    float dh4 = abs(color.z - opt4.z);
	
	if (dh1 < abs(360.0 + color.z - opt1.z))
	{
		dh1 = abs(360.0 + color.z - opt1.z);
	}
	
	if (dh2 < abs(360.0 + color.z - opt2.z))
	{
		dh2 = abs(360.0 + color.z - opt2.z);
	}
	
	if (dh3 < abs(360.0 + color.z - opt3.z))
	{
		dh3 = abs(360.0 + color.z - opt3.z);
	}
	
	if (dh4 < abs(360.0 + color.z - opt4.z))
	{
		dh4 = abs(360.0 + color.z - opt4.z);
	}
	
	if (dh1 < abs(-360.0 + color.z - opt1.z))
	{
		dh1 = abs(-360.0 + color.z - opt1.z);
	}
	
	if (dh2 < abs(-360.0 + color.z - opt2.z))
	{
		dh2 = abs(-360.0 + color.z - opt2.z);
	}
	
	if (dh3 < abs(-360.0 + color.z - opt3.z))
	{
		dh3 = abs(-360.0 + color.z - opt3.z);
	}
	
	if (dh4 < abs(-360.0 + color.z - opt4.z))
	{
		dh4 = abs(-360.0 + color.z - opt4.z);
	}

    if (dh1 < dh2)
    {
        if (dh3 < dh4)
        {
            if (dh1 < dh3)
            {
                return opt1;
            }
            else
            {
                return opt3;
            }
        }
        else
        {
            if (dh1 < dh4)
            {
                return opt1;
            }
            else
            {
                return opt4;
            }
        }
    }
    else
    {
        if (dh3 < dh4)
        {
            if (dh2 < dh3)
            {
                return opt2;
            }
            else
            {
                return opt3;
            }
        }
        else
        {
            if (dh2 < dh4)
            {
                return opt2;
            }
            else
            {
                return opt4;
            }
        }
    }
}







// NOTE(srp): Methods below adapted from https://github.com/colormine/colormine
// under MIT licence.
// Changed some stuff to better match the standards as given by Wikipedia.
vec3 rgb2xyz(vec3 c)
{
    mat3 rgb2xyz = mat3(vec3(0.49, 0.17697, 0.0), vec3(0.31, 0.8124, 0.01), vec3(0.2, 0.01063, 0.99));
    return rgb2xyz * c;
}

vec3 xyz2rgb(vec3 c)
{
    mat3 xyz2rgb = mat3(vec3(2.36461358, -0.51516621, 0.0052037), vec3(-0.8954057, 1.4264081, -0.01440816), vec3(-0.46807328, 0.0887581, 1.00920446));
    return xyz2rgb * c;
}

float pivotXyz(float n)
{
    // Where d = 6/29
    float d3 = 0.00885645167; 
    if(n > d3)
    {
        return pow(n, 0.33333);
    }

    // coef = 1/3d^3
    // addend = 4/29
    float coef = 7.7870370374;
    float addend = 0.13793103448;
    return coef * n + addend;
}

vec3 xyz2lab(vec3 c)
{
    float x = pivotXyz(c.x / 95.0489); // Illuminant D65
    float y = pivotXyz(c.y / 100.00);
    float z = pivotXyz(c.z / 108.8840);

    return vec3(
        116.0 * y - 16.0,
        500.0 * (x - y),
        200.0 * (y - z)
    );
}


float inversePivotXyz(float n)
{
    // value is 6/29
    if (n > 0.20689655172)
    {
        return n*n*n;
    }

    // first num is 3*(6/29)^2
    // second num is 4/29
    return 0.12841854934 * (n - 0.13793103448);
}

vec3 lab2xyz(vec3 c)
{
    float L = (c.x+16.0)/116.0;
    float a = c.y / 500.0;
    float b = -c.z / 200.0;

    return vec3(
        95.0489 * inversePivotXyz(L + a), // Illuminant D65
        100.0 * inversePivotXyz(L),
        108.8840 * inversePivotXyz(L + b)
    );
}

vec3 lab2rgb(vec3 c)
{
    return xyz2rgb(lab2xyz(c));
}

vec3 rgb2lab(vec3 c)
{
    return xyz2lab(rgb2xyz(c));
}

float atan2(float y,float x)
{
	if (x > 0.0)
	{
		return atan(y/x);
	}
	
	if (x < 0.0)
	{
		if (y >= 0.0)
		{
			return atan(y/x) + PI;
		}
		else
		{
			return atan(y/x) - PI;
		}
	}
	
	if (y > 0.0)
	{
		return PI/2.0;
	}
	return -PI/2.0;
}

vec3 lab2lch(vec3 c)
{
	float L = c.x;
	float A = c.y;
	float B = c.z;
    return vec3(L, sqrt((A*A) + (B*B)), degrees(atan2(B,A)));
}

vec3 lch2lab(vec3 c)
{
	float L = c.x;
	float C = c.y;
	float H = c.z;
    return vec3(L, C*cos(radians(H)), C*sin(radians(H)));
}

vec3 rgb2lch(vec3 c)
{
    return lab2lch(rgb2lab(c));
}

vec3 lch2rgb(vec3 c)
{
    return lab2rgb(lch2lab(c));
}

// NOTE(srp): holy grail inputs: SCREEN_TEXTURE, DEPTH_TEXTURE
// NOTE(srp): Transparent objects won't be caught in the SCREEN_TEXTURE
void fragment() {
    vec3 c = textureLod(SCREEN_TEXTURE, SCREEN_UV, 0.0).rgb;

    vec3 c1lch = rgb2lch(my_color1);
    vec3 c2lch = rgb2lch(my_color2);
    vec3 c3lch = rgb2lch(my_color3);
    vec3 c4lch = rgb2lch(my_color4);

    c = rgb2lch(c);
    c = vec3(c.x, c.y, nearestHue(c, c1lch, c2lch, c3lch, c4lch).z);

    COLOR.rgb = lch2rgb(c);

	//COLOR.rgb =  lch2rgb(lab2lch(xyz2lab(rgb2xyz(c))));
}




// Inline function definition template if needed later O.O
/*
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
*/
// Calls happen like this:
// vec3 col1hsv = ¡¡rgb2hsv(my_color1.rgb)¡¡;
