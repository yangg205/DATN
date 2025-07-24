﻿
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using System.Globalization;

public class UTUUtilities : AssetPostprocessor
{
	class Section
	{
		public int Offset;
		public int NumIndices;
		public int Materialindex;
		public string MaterialName;
		public Material Mat;
	}
	class MeshBinding
	{
		public Mesh mesh;
		public MeshRenderer renderer;
	}
	void GetFloatsFromLine( string Line, ref List<float> Floats, GameObject gameobject )
	{
		Floats.Clear();

		string[] Tokens = Line.Split(new char[]{ ' ' } );

		for( int u = 1; u < Tokens.Length; u++ )
		{
			try
			{
				CultureInfo ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
				ci.NumberFormat.CurrencyDecimalSeparator = ".";
				float Val = float.Parse( Tokens[u], ci.NumberFormat);
				Floats.Add( Val );
			}
			catch( FormatException e )
			{
				Debug.LogError( "float.Parse failed in " + gameobject.name + " input=" + Tokens[u] + " exception=" + e.Message );
			}
		}
	}
	void GetIntsFromLine( string Line, ref List<int> Ints, GameObject gameobject )
	{
		Ints.Clear();

		string[] Tokens = Line.Split(new char[]{ ' ', '/' } );

		for( int u = 1; u < Tokens.Length; u++ )
		{
			try
			{
				int Val = int.Parse( Tokens[u] );
				Ints.Add( Val );
			}
			catch( FormatException e )
			{
				Debug.LogError( "int.Parse failed in " + gameobject.name + " input=" + Tokens[u] + " exception=" + e.Message );
			}
		}
	}
	private void OnPostprocessModel( GameObject gameobject )
	{
		UnityEditor.ModelImporter importer = this.assetImporter as UnityEditor.ModelImporter;

		string Path = importer.assetPath;
		if( !Path.Contains( ".obj" ) )
			return;


		string OBJFile = File.ReadAllText( Path );

		List<float> Floats = new List<float>();
		List<int> Ints = new List<int>();

		List<Vector3> Vertices = new List<Vector3>();
		List<Color> Colors = new List<Color>();
		List<Vector2> Texcoords0 = new List<Vector2>();
		List<Vector2> Texcoords1 = new List<Vector2>();
		List<Vector3> Normals = new List<Vector3>();
		List<Vector4> Tangents = new List<Vector4>();
		bool ReplaceNormals = true;
		List<int> Indices = new List<int>();
		List<Section> Sections = new List<Section>();
		string[] Lines = OBJFile.Split( new char[]{ '\n' } );
		for( int i = 0; i < Lines.Length; i++ )
		{
			string Line = Lines[i];
			if( Line.StartsWith( "v " ) )
			{
				string[] Tokens = Line.Split(new char[]{ ' ' } );
				Floats.Clear();
				for( int u = 1; u < Tokens.Length; u++ )//0 should be "v"
				{
					try
					{
						CultureInfo ci = (CultureInfo)CultureInfo.InvariantCulture.Clone();
						ci.NumberFormat.CurrencyDecimalSeparator = ".";
						float Val = float.Parse( Tokens[u], ci.NumberFormat);
						Floats.Add( Val );
					}
					catch( FormatException e )
					{
						Debug.LogError( "float.Parse failed for verts in " + gameobject.name + " input=" + Tokens[u] + " exception=" + e.Message );
					}
				}

				if( Floats.Count >= 3 )
					Vertices.Add( new Vector3( Floats[0], Floats[1], Floats[2] ) );
				else
					Vertices.Add( new Vector3( 0, 0, 0 ) );

				if( Floats.Count == 7 )
				{
					Colors.Add( new Color( Floats[3], Floats[4], Floats[5], Floats[6] ) );
				}
			}
			else if( Line.StartsWith( "vt " ) )
			{
				GetFloatsFromLine( Line, ref Floats, gameobject );
				if( Floats.Count == 2 )
				{
					Texcoords0.Add( new Vector2( Floats[0], Floats[1] ) );
				}
				else
					Texcoords0.Add( new Vector2( 0, 0 ) );
			}
			else if( Line.StartsWith( "vt1 " ) )
			{
				GetFloatsFromLine( Line, ref Floats, gameobject );
				if( Floats.Count == 2 )
				{
					Texcoords1.Add( new Vector2( Floats[0], Floats[1] ) );
				}
				else
					Texcoords1.Add( new Vector2( 0, 0 ) );
			}
			else if( ReplaceNormals && Line.StartsWith( "vn " ) )
			{
				GetFloatsFromLine( Line, ref Floats, gameobject );
				if( Floats.Count == 3 )
				{
					Normals.Add( new Vector3( Floats[0], Floats[1], Floats[2] ) );
				}
				else
					Normals.Add( new Vector3( 0, 0, 0 ) );
			}
			else if( Line.StartsWith( "tan " ) )
			{
				GetFloatsFromLine( Line, ref Floats, gameobject );
				if( Floats.Count == 4 )
				{
					Tangents.Add( new Vector4( Floats[0], Floats[1], Floats[2], Floats[3] ) );
				}
				else
					Normals.Add( new Vector4( 0, 0, 0, 0 ) );
			}
			else if( Line.StartsWith( "f " ) )
			{
				GetIntsFromLine( Line, ref Ints, gameobject );
				if( Ints.Count == 9 )
				{
					Indices.Add( Ints[0] - 1 );
					Indices.Add( Ints[3] - 1 );
					Indices.Add( Ints[6] - 1 );
				}
			}
			//"#Section %d Offset %d NumIndices %d MaterialIndex %d %s\n"
			if( Line.StartsWith( "#Section " ) )
			{
				string[] Tokens = Line.Split(new char[]{ ' ' } );
				if( Tokens.Length == 9 )
				{
					Section NewSection = new Section();
					NewSection.Offset = int.Parse( Tokens[3] );
					NewSection.NumIndices = int.Parse( Tokens[5] );
					//NewSection.Materialindex = int.Parse( Tokens[7] );
					NewSection.MaterialName = Tokens[8].Trim();
					Sections.Add( NewSection );
				}
			}
		}

		MeshBinding[] MeshArray = GetMeshes( gameobject);
		if( MeshArray.Length == 0 )
			return;

		MeshBinding Binding = MeshArray[0];
		Mesh m = Binding.mesh;

		if( m.vertices.Length != Vertices.Count )
		{
			Debug.LogError( "m.vertices.Length != Vertices.Count for " + gameobject.name );
		}
		if( m.GetIndices( 0 ).Length != Indices.Count )
		{
			Debug.LogError( "m.GetIndices( 0 ).Length != Indices.Count for " + gameobject.name );
		}
		m.SetIndices( Indices.ToArray(), MeshTopology.Triangles, 0 );
		m.vertices = Vertices.ToArray();
		if( Texcoords0.Count != m.uv.Length )
		{
			Debug.LogError( "Texcoords0.Count != m.uv.Length for " + gameobject.name );
		}
		m.uv = Texcoords0.ToArray();

		if( Colors.Count > 0 )
		{
			if( m.vertices.Length != Colors.Count )
			{
				Debug.LogError( " Mesh vertex count != color count for " + gameobject.name );
			}
			else
			{
				m.colors = Colors.ToArray();
			}
		}
		if( Texcoords1.Count > 0 )
		{
			if( m.vertices.Length != Texcoords1.Count )
			{
				Debug.LogError( " Mesh vertex count != Texcoords1 count for " + gameobject.name );
			}
			else
			{
				m.uv2 = Texcoords1.ToArray();
			}
		}

		if( Sections.Count > 1 )
		{
			SetupSubmeshes( m, Sections.ToArray(), gameobject.name );
		}

		Material[] UsedMaterials = GetSectionMaterials( Sections.ToArray() );
		Binding.renderer.sharedMaterials = UsedMaterials;

		if( ReplaceNormals && Normals.Count > 0 )
		{
			if( m.vertices.Length != Normals.Count )
			{
				Debug.LogError( " Mesh vertex count != Texcoords1 count " + gameobject.name );
			}
			else
			{
				m.normals = Normals.ToArray();
			}
		}
		if( Tangents.Count > 0 )
		{
			if( m.tangents.Length == 0 || m.tangents.Length == Tangents.Count )
			{
				m.tangents = Tangents.ToArray();
			}
			else
			{
				Debug.LogError( " Mesh vertex count != Tangents count " + gameobject.name );
			}
		}
		m.RecalculateBounds();
	}
	Material[] GetSectionMaterials( Section[] sections )
	{
		Material[] AllMaterials = UnityEngine.Resources.FindObjectsOfTypeAll<Material>();
		Material[] UsedMaterials = new Material[sections.Length];

		for( int i = 0; i < sections.Length; i++ )
		{
			Section s = sections[i];

			if( !s.MaterialName.Equals( "None" ) )
			{
				for( int m = 0; m < AllMaterials.Length; m++ )
				{
					if( AllMaterials[m].name.Equals( s.MaterialName ) )
					{
						s.Mat = AllMaterials[m];
						UsedMaterials[i] = s.Mat;
						break;
					}
				}
			}
		}

		return UsedMaterials;
	}
	void SetupSubmeshes( Mesh mesh, Section[] sections, String Name )
	{
		int[] AllIndices = mesh.GetIndices( 0 );
		mesh.subMeshCount = sections.Length;

		for( int i = 0; i < sections.Length; i++ )
		{
			Section s = sections[i];

			int[] SectionIndices = new int[ s.NumIndices ];

			for( int u = 0; u < s.NumIndices; u++ )
			{
				if( s.Offset + u < AllIndices.Length )
					SectionIndices[u] = AllIndices[s.Offset + u];
				else
				{
					Debug.LogError( "AllIndices is too small for " + Name );
					break;
				}
			}

			mesh.SetIndices( SectionIndices, MeshTopology.Triangles, i );
		}
	}
	MeshBinding[] GetMeshes( GameObject gameObject )
	{
		List<MeshBinding> MeshArray = new List<MeshBinding>();
		MeshFilter[] MFArray = gameObject.GetComponentsInChildren<MeshFilter>();
		if( MFArray != null )
		{
			for( int i = 0; i < MFArray.Length; i++ )
			{
				MeshFilter MF = MFArray[i];
				Mesh m = MFArray[i].sharedMesh;

				MeshBinding NewMeshBinding = new MeshBinding();
				NewMeshBinding.mesh = m;
				NewMeshBinding.renderer = MF.gameObject.GetComponent<MeshRenderer>();
				MeshArray.Add( NewMeshBinding );
			}
		}

		return MeshArray.ToArray();
	}
	[MenuItem( "UTU/PrintMesh" )]
	static void PrintMesh()
	{
		if( Selection.gameObjects.Length == 0 )
			return;
		GameObject go = Selection.gameObjects[0];

		MeshFilter MF = go.GetComponent<MeshFilter>();
		Mesh mesh = MF.sharedMesh;

		string Data = "";
		Data += "Vertices " + mesh.vertexCount + "\n";

		for( int i = 0; i < mesh.subMeshCount; i++ )
		{
			int[] Indices = mesh.GetTriangles( i );

			uint BaseVertex = mesh.GetBaseVertex( i );
			Data += "Section " + i + " Indices " + Indices.Length + " BaseVertex " + BaseVertex + "\n";
		}

		Vector3[] vertices = mesh.vertices;
		for( int i = 0; i < vertices.Length; i++ )
		{
			Data += "v " + vertices[i].x + " " + vertices[i].y + " " + vertices[i].z + "\n";
		}

		for( int i = 0; i < mesh.subMeshCount; i++ )
		{
			int[] Indices = mesh.GetTriangles( i );
			Data += "Section " + i + "\n";
			for( int u = 0; u < Indices.Length; u += 3 )
			{
				int I1 = Indices[ u ]    + 1;
				int I2 = Indices[ u + 1] + 1;
				int I3 = Indices[ u + 2] + 1;
				//Debug.Log( "f " + I1 + " " + I2 + " " + I3 );
				Data += "f " + I3 + " " + I2 + " " + I1 + "\n";
			}
		}

		File.WriteAllText( "C:/UnrealToUnity/Out.txt", Data );
	}
	[MenuItem( "UTU/DisableAllLights" )]
	static void DisableAllLights()
	{
		UnityEngine.Object[] AllLights = Resources.FindObjectsOfTypeAll( typeof( Light ) );

		for(int i=0; i< AllLights.Length; i++ )
        {
			Light L = AllLights[i] as Light;
			L.enabled = false;
		}
	}
	[MenuItem( "UTU/SetBounds" )]
	static void SetBounds()
	{
		MeshRenderer[] AllMeshRenderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();

		for(int i=0; i< AllMeshRenderers.Length; i++ )
        {
			MeshRenderer MR = AllMeshRenderers[i];
			if( MR == null )
				continue;

			MeshFilter MF = MR.gameObject.GetComponent<MeshFilter>();
			if( MF == null || MF.sharedMesh == null )
				return;
			
			Vector3 LocalObjectBoundsMin = MF.sharedMesh.bounds.min;
			Vector3 LocalObjectBoundsMax = MF.sharedMesh.bounds.max;

			//To Unreal units
			LocalObjectBoundsMin.Set( LocalObjectBoundsMin.x * 100, LocalObjectBoundsMin.z * 100, LocalObjectBoundsMin.y * 100 );
			LocalObjectBoundsMax.Set( LocalObjectBoundsMax.x * 100, LocalObjectBoundsMax.z * 100, LocalObjectBoundsMax.y * 100 );

			Material[] Mats = MR.sharedMaterials;
			if( Mats != null )
			{
				for( int u = 0; u < Mats.Length; u++ )
				{
					Material mat = Mats[u];
					mat.SetVector( "LocalObjectBoundsMin", LocalObjectBoundsMin );
					mat.SetVector( "LocalObjectBoundsMax", LocalObjectBoundsMax );
				}
			}
		}
	}
	static Vector3 GetPosition( Matrix4x4 mat )
    {
		return new Vector3( mat.m03, mat.m13, mat.m23 );
    }
	[MenuItem( "UTU/MoveMeshesToTerrainComponent" )]
	static void MoveMeshesToTerrainComponent()
	{
		Terrain[] AllTerrains = UnityEngine.Object.FindObjectsOfType<Terrain>();

		if( AllTerrains.Length == 0 )
			return;

		Terrain  SelectedTerrain = AllTerrains[0];
		TreePrototype[] TreePrototypes = SelectedTerrain.terrainData.treePrototypes;

		GameObject[] AllGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

		TreeInstance[] CurrentInstances = SelectedTerrain.terrainData.treeInstances;
		List<TreeInstance> CurrentInstancesList = new List<TreeInstance>();
		for(int i=0; i< CurrentInstances.Length; i++ )
        {
			CurrentInstancesList.Add( CurrentInstances[i] );
		}

		bool Quit = false;
		if( Quit )
			return;

		for(int i=0; i< TreePrototypes.Length; i++ )
        {
			var Prototype = TreePrototypes[ i ];

			if( Prototype.prefab == null )
				continue;

			for(int u=0; u< AllGameObjects.Length; u++)
            {
				var GO = AllGameObjects[u];

				var type = PrefabUtility.GetPrefabAssetType( GO );
				if( type == PrefabAssetType.Regular )
				{
					var PrefabSource = PrefabUtility.GetCorrespondingObjectFromSource( GO );

					if( PrefabSource == Prototype.prefab )
					{
						TreeInstance NewInstance = new TreeInstance();
						NewInstance.prototypeIndex = i;

						//Move Gameobject in terrain space
						Matrix4x4 Mat = GO.transform.localToWorldMatrix;
						Matrix4x4 TerrainMatInv = SelectedTerrain.gameObject.transform.localToWorldMatrix.inverse;// * SelectedTerrain.gameObject.transform.
						Matrix4x4 FinalMat = TerrainMatInv * Mat;
						Vector3 FinalPosition = GetPosition( FinalMat );
						
						//var InTerrainSpace = GO.transform.localToWorldMatrix * SelectedTerrain.gameObject.transform;
						float X = FinalPosition.x / SelectedTerrain.terrainData.size.x;
						float Y = FinalPosition.y / SelectedTerrain.terrainData.size.y;
						float Z = FinalPosition.z / SelectedTerrain.terrainData.size.z;
						NewInstance.position = new Vector3( X, Y, Z );
						float OneDegreeAngle = ((float)Math.PI) / 180.0f;
						NewInstance.rotation = FinalMat.rotation.eulerAngles.y * OneDegreeAngle;
						NewInstance.heightScale = GO.transform.localScale.y;
						NewInstance.widthScale = ( GO.transform.localScale.x + GO.transform.localScale.z ) / 2;
						NewInstance.color = NewInstance.lightmapColor = new Color32( 255, 255, 255, 255 );
						CurrentInstancesList.Add( NewInstance );

						bool DeleteGameObject = true;
						if ( DeleteGameObject )
                        {
							UnityEngine.Object.DestroyImmediate( GO );
						}
					}
				}
			}			
		}

		SelectedTerrain.terrainData.treeInstances = CurrentInstancesList.ToArray();
	}
	static GameObject AddLodGroupToPrefabRoot( GameObject Prefab )
	{
		LODGroup Lods = Prefab.GetComponent<LODGroup>();
		if( Lods == null )
		{
			//Add a lodgroup manually since objects with a single LOD won't get rendered by the terrain system
			Lods = Prefab.AddComponent<LODGroup>();
			MeshRenderer MR = Prefab.GetComponentInChildren<MeshRenderer>();
			if( MR != null )
			{
				LOD NewLOD = new LOD();
				NewLOD.renderers = new Renderer[] { MR };
				LOD[] AllLods = new LOD[]{ NewLOD };
				Lods.SetLODs( AllLods );
			}

			return PrefabUtility.SavePrefabAsset( Prefab );
		}
		else
			return Prefab;
	}
	[MenuItem( "UTU/MoveTreeDataToTerrainComponent" )]
	static void MoveTreeDataToTerrainComponent()
	{
		Terrain[] AllTerrains = UnityEngine.Object.FindObjectsOfType<Terrain>();

		if( AllTerrains.Length == 0 )
			return;

		Terrain  SelectedTerrain = AllTerrains[0];
		TreePrototype[] TreePrototypes = SelectedTerrain.terrainData.treePrototypes;
		TreeInstanceComponent[] TreeInstanceComponents = UnityEngine.Object.FindObjectsOfType<TreeInstanceComponent>();
		if( TreeInstanceComponents.Length == 0 )
			return;

		TreeInstanceComponent TreeInstances = TreeInstanceComponents[0];

		TreeInstance[] CurrentInstances = SelectedTerrain.terrainData.treeInstances;
		List<TreeInstance> CurrentInstancesList = new List<TreeInstance>();
		List<TreePrototype> TreePrototypesList = new List<TreePrototype>();
		for( int i = 0; i < CurrentInstances.Length; i++ )
		{
			CurrentInstancesList.Add( CurrentInstances[i] );
		}

		bool Quit = false;
		if( Quit )
			return;

		for( int i = 0; i < TreeInstances.Prefab.Count; i++ )
		{
			GameObject TreePrefab = TreeInstances.Prefab[ i ];

			if( TreePrefab == null )
				continue;

			GameObject SavedPrefab = AddLodGroupToPrefabRoot( TreePrefab );
			if( SavedPrefab )
				TreePrefab = SavedPrefab;

			TreePrototype Prototype = null;
			int prototypeIndex = -1;
			for(int u=0; u< TreePrototypesList.Count; u++)
            {
				if ( TreePrototypesList[u].prefab == TreePrefab)
                {
					Prototype = TreePrototypesList[u];
					prototypeIndex = u;
					break;
				}
            }

			if ( Prototype  == null)
            {
				Prototype = new TreePrototype();
				Prototype.prefab = TreePrefab;
				prototypeIndex = TreePrototypesList.Count;
				TreePrototypesList.Add( Prototype );
			}

			TreeInstance NewInstance = new TreeInstance();
			NewInstance.prototypeIndex = prototypeIndex;

			Quaternion Quat = Quaternion.Euler( new Vector3( 0, TreeInstances.rotation[i], 0 ) );
			Matrix4x4 WorldMatrix = Matrix4x4.TRS( TreeInstances.position[i], Quat, new Vector3( TreeInstances.widthScale[i], TreeInstances.heightScale[i], TreeInstances.widthScale[i] ) );

			//Move Gameobject in terrain space
			Matrix4x4 Mat = WorldMatrix;
			Matrix4x4 TerrainMatInv = SelectedTerrain.gameObject.transform.localToWorldMatrix.inverse;// * SelectedTerrain.gameObject.transform.
			Matrix4x4 FinalMat = TerrainMatInv * Mat;
			Vector3 FinalPosition = GetPosition( FinalMat );

			//var InTerrainSpace = GO.transform.localToWorldMatrix * SelectedTerrain.gameObject.transform;
			float X = FinalPosition.x / SelectedTerrain.terrainData.size.x;
			float Y = FinalPosition.y / SelectedTerrain.terrainData.size.y;
			float Z = FinalPosition.z / SelectedTerrain.terrainData.size.z;
			NewInstance.position = new Vector3( X, Y, Z );
			float OneDegreeAngle = ((float)Math.PI) / 180.0f;
			NewInstance.rotation = FinalMat.rotation.eulerAngles.y * OneDegreeAngle;
			NewInstance.heightScale = TreeInstances.heightScale[i];
			NewInstance.widthScale = TreeInstances.widthScale[i];
			NewInstance.color = NewInstance.lightmapColor = new Color32( 255, 255, 255, 255 );
			CurrentInstancesList.Add( NewInstance );					
		}

		SelectedTerrain.terrainData.treePrototypes = TreePrototypesList.ToArray();
		SelectedTerrain.terrainData.treeInstances = CurrentInstancesList.ToArray();		
	}
	[MenuItem( "UTU/FixSkinnedMeshes" )]
	static void FixSkinnedMeshes()
	{
		AnimationClip[] AllAnimations = UnityEngine.Resources.FindObjectsOfTypeAll<AnimationClip>();
		SkinnedMeshRenderer[] AllSMR = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();

		for(int i=0; i<AllSMR.Length;i++ )
        {
			SkinnedMeshRenderer SMR = AllSMR[i];
			if( SMR == null || SMR.sharedMesh == null )
				continue;

			var path = AssetDatabase.GetAssetPath(SMR.sharedMesh);
			UnityEngine.Object mainAssetFile = AssetDatabase.LoadMainAssetAtPath(path);
			GameObject RootGO = UnityEngine.Object.Instantiate( mainAssetFile, new Vector3( 0, 0, 0 ), Quaternion.identity ) as GameObject;

			var InstanceSMR = RootGO.GetComponentInChildren<SkinnedMeshRenderer>();
			InstanceSMR.sharedMaterials = SMR.sharedMaterials;
			Animation AnimComp = RootGO.AddComponent<Animation>();

			if( AllAnimations.Length > 1 )
			{
				AnimComp.clip = AllAnimations[1];
			}
		}
	}
}
#endif