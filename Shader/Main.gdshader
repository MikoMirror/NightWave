shader_type spatial;

uniform sampler2D texture_albedo : hint_albedo;
uniform sampler2D texture_normal : hint_normal;
uniform sampler2D texture_roughness : hint_roughness;

uniform float pencil_thickness = 0.05;
uniform float pencil_pressure = 1.0;
uniform vec3 pencil_color = vec3(0.0, 0.0, 0.0);

void fragment() {
  // Sample textures
  vec3 albedo = texture(texture_albedo, UV).rgb;
  vec3 normal = texture(texture_normal, UV).rgb * 2.0 - 1.0;
  float roughness = texture(texture_roughness, UV).r;

  // Calculate view direction
  vec3 view_direction = normalize(VIEW_MATRIX * vec4(0.0, 0.0, 1.0, 0.0)).xyz;

  // Calculate light direction
  vec3 light_direction = normalize(LIGHT0_DIRECTION);

  // Calculate diffuse lighting
  float diffuse_intensity = max(dot(normal, light_direction), 0.0);

  // Calculate pencil effect
  float distance_to_center = distance(UV, vec2(0.5, 0.5));
  float pencil_opacity = clamp(1.0 - distance_to_center / pencil_thickness, 0.0, 1.0);
  pencil_opacity *= pencil_pressure;

  // Blend pencil color with albedo
  vec3 final_color = mix(albedo, pencil_color, pencil_opacity);

  // Apply lighting
  final_color *= diffuse_intensity;

  // Output color
  COLOR = vec4(final_color, 1.0);
}