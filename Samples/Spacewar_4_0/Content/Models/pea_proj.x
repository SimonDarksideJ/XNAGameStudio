xof 0303txt 0032
//
// DirectX file: \\cpatgfsa01\atg\Graphics-Samples-ATG_Content\XNA_Spacewar\Maya\OBJs\projectiles\pea_proj.x
//
// Converted by the PolyTrans geometry converter from Okino Computer Graphics, Inc.
// Date/time of export: 06/28/2006 17:15:05
//
// Bounding box of geometry = (-100,-100,-100) to (100,100,100).


template Header {
 <3D82AB43-62DA-11cf-AB39-0020AF71E433>
 WORD major;
 WORD minor;
 DWORD flags;
}

template Vector {
  <3D82AB5E-62DA-11cf-AB39-0020AF71E433>
 FLOAT x;
 FLOAT y;
 FLOAT z;
}

template Coords2d {
  <F6F23F44-7686-11cf-8F52-0040333594A3>
 FLOAT u;
 FLOAT v;
}

template Matrix4x4 {
  <F6F23F45-7686-11cf-8F52-0040333594A3>
 array FLOAT matrix[16];
}

template ColorRGBA {
  <35FF44E0-6C7C-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
 FLOAT alpha;
}

template ColorRGB {
 <D3E16E81-7835-11cf-8F52-0040333594A3>
 FLOAT red;
 FLOAT green;
 FLOAT blue;
}

template IndexedColor {
 <1630B820-7842-11cf-8F52-0040333594A3>
DWORD index;
 ColorRGBA indexColor;
}

template Boolean {
 <4885AE61-78E8-11cf-8F52-0040333594A3>
WORD truefalse;
}

template Boolean2d {
 <4885AE63-78E8-11cf-8F52-0040333594A3>
Boolean u;
 Boolean v;
}

template MaterialWrap {
 <4885AE60-78E8-11cf-8F52-0040333594A3>
Boolean u;
 Boolean v;
}

template TextureFilename {
 <A42790E1-7810-11cf-8F52-0040333594A3>
 STRING filename;
}

template Material {
 <3D82AB4D-62DA-11cf-AB39-0020AF71E433>
 ColorRGBA faceColor;
 FLOAT power;
 ColorRGB specularColor;
 ColorRGB emissiveColor;
 [...]
}

template MeshFace {
 <3D82AB5F-62DA-11cf-AB39-0020AF71E433>
 DWORD nFaceVertexIndices;
 array DWORD faceVertexIndices[nFaceVertexIndices];
}

template MeshFaceWraps {
 <4885AE62-78E8-11cf-8F52-0040333594A3>
 DWORD nFaceWrapValues;
 Boolean2d faceWrapValues;
}

template MeshTextureCoords {
 <F6F23F40-7686-11cf-8F52-0040333594A3>
 DWORD nTextureCoords;
 array Coords2d textureCoords[nTextureCoords];
}

template MeshMaterialList {
 <F6F23F42-7686-11cf-8F52-0040333594A3>
 DWORD nMaterials;
 DWORD nFaceIndexes;
 array DWORD faceIndexes[nFaceIndexes];
 [Material]
}

template MeshNormals {
 <F6F23F43-7686-11cf-8F52-0040333594A3>
 DWORD nNormals;
 array Vector normals[nNormals];
 DWORD nFaceNormals;
 array MeshFace faceNormals[nFaceNormals];
}

template MeshVertexColors {
 <1630B821-7842-11cf-8F52-0040333594A3>
 DWORD nVertexColors;
 array IndexedColor vertexColors[nVertexColors];
}

template Mesh {
 <3D82AB44-62DA-11cf-AB39-0020AF71E433>
 DWORD nVertices;
 array Vector vertices[nVertices];
 DWORD nFaces;
 array MeshFace faces[nFaces];
 [...]
}

template FrameTransformMatrix {
 <F6F23F41-7686-11cf-8F52-0040333594A3>
 Matrix4x4 frameMatrix;
}

template Frame {
 <3D82AB46-62DA-11cf-AB39-0020AF71E433>
 [...]
}

Header {
	1; // Major version
	0; // Minor version
	1; // Flags
}

Material xof_default {
	0.400000;0.400000;0.400000;1.000000;;
	32.000000;
	0.700000;0.700000;0.700000;;
	0.000000;0.000000;0.000000;;
}

Material lambert2SG {
	1.0;1.0;1.0;1.000000;;
	32.000000;
	0.000000;0.000000;0.000000;;
	0.000000;0.000000;0.000000;;
	TextureFilename {
		"..\\textures\\pea_proj.tga";
	}
}
Mesh single_mesh_object {
	23;		// 23 vertices
	-100.000000;-0.000009;0.000000;,
	-70.710777;-0.000008;-70.710709;,
	-70.710678;-0.000008;70.710678;,
	-0.000004;100.000000;0.000000;,
	-0.000003;70.710678;70.710678;,
	-0.000003;70.710777;-70.710709;,
	0.000000;0.000000;-99.999969;,
	0.000000;0.000000;-99.999969;,
	0.000000;0.000000;-99.999969;,
	0.000000;0.000000;-99.999969;,
	0.000000;0.000000;100.000000;,
	0.000000;0.000000;100.000000;,
	0.000000;0.000000;100.000000;,
	0.000000;0.000000;100.000000;,
	0.000009;-70.710770;-70.710709;,
	0.000009;-70.710770;-70.710709;,
	0.000009;-70.710678;70.710678;,
	0.000009;-70.710678;70.710678;,
	0.000013;-100.000000;0.000000;,
	0.000013;-100.000000;0.000000;,
	70.710678;0.000000;70.710678;,
	70.710777;0.000000;-70.710709;,
	100.000000;0.000000;0.000000;;

	24;		// 24 faces
	3;17,2,0;,
	3;17,0,19;,
	3;2,4,3;,
	3;2,3,0;,
	3;4,20,22;,
	3;4,22,3;,
	3;20,16,18;,
	3;20,18,22;,
	3;19,0,1;,
	3;19,1,15;,
	3;0,3,5;,
	3;0,5,1;,
	3;3,22,21;,
	3;3,21,5;,
	3;22,18,14;,
	3;22,14,21;,
	3;2,17,13;,
	3;4,2,12;,
	3;20,4,11;,
	3;16,20,10;,
	3;15,1,9;,
	3;1,5,8;,
	3;5,21,7;,
	3;21,14,6;;

	MeshMaterialList {
		1;1;0;;
		{lambert2SG}
	}

	MeshNormals {
		14; // 14 normals
		-1.000000;0.000000;0.000001;,
		-0.671273;0.000000;-0.741210;,
		-0.671273;0.000000;0.741210;,
		0.000000;-1.000000;0.000001;,
		0.000000;-0.671273;-0.741210;,
		0.000000;-0.671273;0.741210;,
		0.000000;0.000000;-1.000000;,
		0.000000;0.000000;1.000000;,
		0.000000;0.671273;-0.741210;,
		0.000000;0.671273;0.741210;,
		0.000000;1.000000;0.000001;,
		0.671273;0.000000;-0.741210;,
		0.671273;0.000000;0.741210;,
		1.000000;0.000000;0.000001;;

		24;		// 24 faces
		3;5,2,0;,
		3;5,0,3;,
		3;2,9,10;,
		3;2,10,0;,
		3;9,12,13;,
		3;9,13,10;,
		3;12,5,3;,
		3;12,3,13;,
		3;3,0,1;,
		3;3,1,4;,
		3;0,10,8;,
		3;0,8,1;,
		3;10,13,11;,
		3;10,11,8;,
		3;13,3,4;,
		3;13,4,11;,
		3;2,5,7;,
		3;9,2,7;,
		3;12,9,7;,
		3;5,12,7;,
		3;4,1,6;,
		3;1,8,6;,
		3;8,11,6;,
		3;11,4,6;;
	}  // End of Normals

	MeshTextureCoords {
		23; // 23 texture coords
		0.250000;0.500000;,
		0.250000;0.250000;,
		0.250000;0.750000;,
		0.500000;0.500000;,
		0.500000;0.750000;,
		0.500000;0.250000;,
		0.875000;0.000000;,
		0.625000;0.000000;,
		0.375000;0.000000;,
		0.125000;0.000000;,
		0.875000;1.000000;,
		0.625000;1.000000;,
		0.375000;1.000000;,
		0.125000;1.000000;,
		1.000000;0.250000;,
		0.000000;0.250000;,
		1.000000;0.750000;,
		0.000000;0.750000;,
		1.000000;0.500000;,
		0.000000;0.500000;,
		0.750000;0.750000;,
		0.750000;0.250000;,
		0.750000;0.500000;;
	}  // End of texture coords
} // End of Mesh
