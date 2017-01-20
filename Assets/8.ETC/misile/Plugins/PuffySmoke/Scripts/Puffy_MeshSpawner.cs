using System;
using System.Collections.Generic;
using UnityEngine;


	public class Puffy_MeshSpawner : Puffy_ShapeSpawner
	{
		public bool smoothNormals = true;

		private Mesh mesh;
		private bool hasColors = false;

		private void Start ()
		{
			mesh = GetComponent<MeshFilter> ().sharedMesh;

			hasColors = mesh.colors.Length > 0;
			emitter.Link (this);
		
			Init ();
		}

		public override void Init ()
		{
			vertices = mesh.vertices;
			normals = mesh.normals;
			colors = mesh.colors;

			if (smoothNormals)
				Smooth ();
			else
			{
				lastSpawnPosition = new Vector3[vertices.Length];
				lastSpawnDirection = new Vector3[vertices.Length];
				
				Matrix4x4 matrix = _transform.localToWorldMatrix;
				for (int i = 0; i < vertices.Length; i++)
				{
					lastSpawnPosition[i] = matrix.MultiplyPoint3x4 (vertices[i]);
					lastSpawnDirection[i] = matrix.MultiplyVector (normals[i]);
				}
			}
		}

		private void Smooth ()
		{
			Dictionary<Vector3, VertexData> data = new Dictionary<Vector3, VertexData> ();
			
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3 vertex = vertices[i];
				Vector3 vertexKey;
				vertexKey.x = (float)Math.Round((double)vertex.x,2);
				vertexKey.y = (float)Math.Round((double)vertex.y,2);
				vertexKey.z = (float)Math.Round((double)vertex.z,2);
			
				VertexData vertexData;
				if (data.TryGetValue (vertexKey, out vertexData))
				{
					vertexData.normal += normals[i];
					if (hasColors)
						vertexData.color += colors[i];
					vertexData.count++;
				}
				else
					data.Add (
						vertexKey,
						new VertexData (vertex, normals[i], hasColors ? colors[i] : Color.white));
			}

			int count = data.Count;
			Array.Resize (ref vertices, count);
			Array.Resize (ref normals, count);
			Array.Resize (ref colors, count);
			Array.Resize (ref lastSpawnPosition, count);
			Array.Resize (ref lastSpawnDirection, count);

			int j = 0;
			Matrix4x4 matrix = transform.localToWorldMatrix;
			foreach (VertexData value in data.Values)
			{
				lastSpawnPosition[j] = matrix.MultiplyPoint3x4 (vertices[j] = value.vertex);
				if (hasColors)
					colors[j] = value.color / value.count;
				lastSpawnDirection[j] = matrix.MultiplyVector (normals[j] = value.normal / value.count);
				j++;
			}
		}

		private class VertexData
		{
			public Vector3 vertex = Vector3.zero;
			public Vector3 normal = Vector3.zero;
			public Color color = Color.white;
			public int count = 1;

			public VertexData (Vector3 v, Vector3 n, Color c)
			{
				vertex = v;
				normal = n;
				color = c;
				count = 1;
			}
		}
	}
