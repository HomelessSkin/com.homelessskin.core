void Billboard_float(in float3 ObjectPosition, in float3 VertexPosition, in float3 CameraPosition, in float3 up, out float3 BillboardPosition)
{
	float3 toCamera = normalize(CameraPosition - ObjectPosition);
    toCamera.y = 0;

    float3 right = normalize(cross(up, normalize(toCamera)));
    
    float3x3 rotMatrix = float3x3(
        right.x, up.x, toCamera.x,
        0, up.y, 0,
        right.z, up.z, toCamera.z
    );
    
    BillboardPosition = mul(rotMatrix, VertexPosition) + ObjectPosition;
}

void Billboard_half(in float3 ObjectPosition, in float3 VertexPosition, in float3 CameraPosition, in float3 up, out float3 BillboardPosition)
{
	float3 toCamera = normalize(CameraPosition - ObjectPosition);
    toCamera.y = 0;

    float3 right = normalize(cross(up, normalize(toCamera)));
    
    float3x3 rotMatrix = float3x3(
        right.x, up.x, toCamera.x,
        0, up.y, 0,
        right.z, up.z, toCamera.z
    );
    
    BillboardPosition = mul(rotMatrix, VertexPosition) + ObjectPosition;
}