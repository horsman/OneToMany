﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Klak.Math;

public class ShaderPath : MonoBehaviour {

    public float scaleFactor = 1f;
    public AnimationCurve blendCurve;

    Mesh mesh;
    Vector3[] origMeshVertices;
    Vector3[] origMeshVerticesInWorldSpace;
    Vector3[] origNormals;
    Graph graph;

    int targetMeshVertexIndex;
    Vector3 targetMeshVertex;

	void Start ()
    {
		mesh = GetComponent<MeshFilter>().mesh;
		origMeshVertices = mesh.vertices;
		origNormals = mesh.normals;
        targetMeshVertexIndex = 0;

        origMeshVerticesInWorldSpace = new Vector3[origMeshVertices.Length];
		for (int i = 0; i < origMeshVertices.Length; i++)
		{
			origMeshVerticesInWorldSpace[i] = transform.TransformPoint(origMeshVertices[i]);
		}

        //// Build graph
        graph = new Graph();
        graph.Initialize(mesh);

        //// Vertex colors
        Color[] heatmap = new Color[4];
        heatmap[0] = new Color(0f,   0f, 0f, 1f);   // black
        heatmap[1] = new Color(0.3f, 0f, 0f, 1f);   // dark red
        heatmap[2] = new Color(0.6f, 0f, 0f, 1f);   // med red
        heatmap[3] = new Color(1f,   0f, 0f, 1f);   // bright red

		Color[] colors = new Color[origMeshVertices.Length];
		for (int i = 0; i < origMeshVertices.Length; i++)
        {
            colors[i] = heatmap[0];
        }

        List<int> visited = new List<int>();
        var gv0 = graph.meshToGraphVertexDict[targetMeshVertexIndex];
        foreach (var meshIndex in gv0.meshIndices)
        {
            colors[meshIndex] = heatmap[3];
            visited.Add(meshIndex);
        }

        foreach (var neighbour in gv0.neighbours)
        {
            foreach (var i in neighbour.meshIndices)
            {
				colors[i] = heatmap[2];
				visited.Add(i);
            }

            foreach (var secondNeighbour in neighbour.neighbours)
			{
                foreach (var i in secondNeighbour.meshIndices)
                {
					if (!visited.Contains(i))
					{
						colors[i] = heatmap[1];
						visited.Add(i);
					}
                }

			}
        }
        mesh.colors = colors;
	}

	void Update()
	{
		var vertices = mesh.vertices;
        var uvs = mesh.uv;
		var normals = mesh.normals;
        targetMeshVertex = vertices[targetMeshVertexIndex];

		for (int i = 0; i < vertices.Length; i++)
		{
            var noise = (0.5f + 0.5f * Perlin.Noise(
                    Time.time + origMeshVerticesInWorldSpace[i].x,
                    Time.time + origMeshVerticesInWorldSpace[i].y,
                    Time.time + origMeshVerticesInWorldSpace[i].z
            ));
                         
			vertices[i] = origMeshVertices[i] +
			  origNormals[i] * scaleFactor * mesh.colors[i].r * noise;
		}
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}
}
