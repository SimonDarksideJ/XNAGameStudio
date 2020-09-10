xof 0303txt 0032
template FVFData {
 <b6e70a0e-8ef9-4e83-94ad-ecc8b0c04897>
 DWORD dwFVF;
 DWORD nDWords;
 array DWORD data[nDWords];
}

template EffectInstance {
 <e331f7e4-0559-4cc2-8e99-1cec1657928f>
 STRING EffectFilename;
 [...]
}

template EffectParamFloats {
 <3014b9a0-62f5-478c-9b86-e4ac9f4e418b>
 STRING ParamName;
 DWORD nFloats;
 array FLOAT Floats[nFloats];
}

template EffectParamString {
 <1dbc4c88-94c1-46ee-9076-2c28818c9481>
 STRING ParamName;
 STRING Value;
}

template EffectParamDWord {
 <e13963bc-ae51-4c5d-b00f-cfa3a9d97ce5>
 STRING ParamName;
 DWORD Value;
}

Material PrivacyGlassMaterial {
 1.000000;1.000000;1.000000;1.000000;;
 0.000000;
 1.000000;1.000000;1.000000;;
 0.000000;0.000000;0.000000;;

 EffectInstance {
  "Distorters.fx";
  
  EffectParamString {
   "DisplacementMap";
   "PrivacyGlass.png";
  }
 }
}


Frame Plane01 {
 

 FrameTransformMatrix {
  1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000,0.000000,0.000000,0.000000,0.000000,1.000000;;
 }

 Mesh  {
  4;
  -0.500000;-0.500000;0.000000;,
  0.500000;-0.500000;0.000000;,
  -0.500000;0.500000;0.000000;,
  0.500000;0.500000;0.000000;;
  4;
  3;2,0,3;,
  3;1,3,0;,
  3;3,0,2;,
  3;0,3,1;;

  MeshNormals  {
   1;
   0.000000;0.000000;1.000000;;
   4;
   3;0,0,0;,
   3;0,0,0;,
   3;0,0,0;,
   3;0,0,0;;
   ;
  }

  MeshTextureCoords  {
   4;
   0.000000;1.000000;,
   1.000000;1.000000;,
   0.000000;0.000000;,
   1.000000;0.000000;;
  }
  
  MeshMaterialList {
	1;
	4;
	0,
	0,
	0,
	0;
	{ PrivacyGlassMaterial }
  }
 }
}