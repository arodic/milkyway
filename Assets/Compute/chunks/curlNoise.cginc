float3 snoise3( float3 x ){
  float noise1 = snoise( x );
  float noise2 = snoise( float3( x.y - 23.7 , x.z + 63.2 , x.x + 135.4 ));
  float noise3 = snoise( float3( x.z + 95.3 , x.x - 20.5 , x.y + 219.1 ));
  float3 finalNoiseVec = float3( noise1 , noise2 , noise3 );
  return finalNoiseVec;
}

float3 curlNoise( float3 p ){
  const float dif = .1;
  float3 dx = float3( dif , 0 , 0 );
  float3 dy = float3( 0 , dif , 0 );
  float3 dz = float3( 0 , 0 , dif );

  float3 doX = snoise3( p - dx );
  float3 upX = snoise3( p + dx );
  float3 doY = snoise3( p - dy );
  float3 upY = snoise3( p + dy );
  float3 doZ = snoise3( p - dz );
  float3 upZ = snoise3( p + dz );

  float finalDifX = upY.z - doY.z - upZ.y + doZ.y;
  float finalDifY = upX.x - doZ.x - upX.z + doX.z;
  float finalDifZ = upX.y - doX.y - upY.x + doY.x;

  const float divisor = 1.0 / ( 2.0 * dif );
  return normalize( float3( finalDifX , finalDifY , finalDifZ ) * divisor );
}
