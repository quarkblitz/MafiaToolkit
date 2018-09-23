﻿using Fbx;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Mafia2.FBX
{
    public class FbxElement
    {
        FbxModel model;
        FbxGeometry geometry;
        Dictionary<int, FbxVideo> videos = new Dictionary<int, FbxVideo>();
        Dictionary<int, FbxMaterial> materials = new Dictionary<int, FbxMaterial>();
        Dictionary<int, FbxTexture> textures = new Dictionary<int, FbxTexture>();
        Lod lod;

        public FbxModel Model {
            get { return model; }
            set { model = value; }
        }
        public FbxGeometry Geometry {
            get { return geometry; }
            set { geometry = value; }
        }
        public Dictionary<int, FbxVideo> Videos {
            get { return videos; }
            set { videos = value; }
        }
        public Dictionary<int, FbxMaterial> Materials {
            get { return materials; }
            set { materials = value; }
        }
        public Dictionary<int, FbxTexture> Textures {
            get { return textures; }
            set { textures = value; }
        }

        public Lod BuildM2ModelFromElement()
        {
            lod = new Lod();
            lod.VertexDeclaration = VertexFlags.Position;
            lod.Vertices = ConvertFBXVertices();

            if (geometry.LayerNormal != null)
                lod.VertexDeclaration |= VertexFlags.Normals;

            if (geometry.LayerUV != null)
                lod.VertexDeclaration |= VertexFlags.TexCoords0;

            lod.Parts = new ModelPart[materials.Count];

            Short3[] allTriangles = ConvertFBXTriangles();
            List<List<Short3>> partTriangles = new List<List<Short3>>();

            for (int i = 0; i != lod.Parts.Length; i++)
                partTriangles.Add(new List<Short3>());

            for (int i = 0; i != allTriangles.Length; i++)
                partTriangles[geometry.LayerMaterial.Materials[i]].Add(allTriangles[i]);

            for (int i = 0; i != lod.Parts.Length; i++)
            {
                lod.Parts[i] = new ModelPart();
                lod.Parts[i].Material = materials.ElementAt(i).Value.Name.Split(new char[] {':' }, StringSplitOptions.None)[1];
                lod.Parts[i].Indices = partTriangles[i].ToArray();
                lod.Parts[i].CalculatePartBounds(lod.Vertices);
            }
            return lod;
        }

        private Vertex[] ConvertFBXVertices()
        {
            Vertex[] vertices = new Vertex[geometry.Vertices.Length/3];
            
            int vertIndex = 0;
            int normalIndex = 0;
            int uvIndex = 0;
            for(int i = 0; i != vertices.Length; i++)
            {
                vertices[i] = new Vertex();
                vertices[i].Position.X = Convert.ToSingle(geometry.Vertices[vertIndex]);
                vertices[i].Position.Y = Convert.ToSingle(geometry.Vertices[++vertIndex]);
                vertices[i].Position.Z = Convert.ToSingle(geometry.Vertices[++vertIndex]);
                vertices[i].Normal.X = Convert.ToSingle(geometry.LayerNormal.Normals[normalIndex]);
                vertices[i].Normal.Y = Convert.ToSingle(geometry.LayerNormal.Normals[++normalIndex]);
                vertices[i].Normal.Z = Convert.ToSingle(geometry.LayerNormal.Normals[++normalIndex]);
                vertices[i].UVs[0].X = HalfHelper.SingleToHalf(Convert.ToSingle(geometry.LayerUV.UVs[uvIndex]));
                vertices[i].UVs[0].Y = HalfHelper.SingleToHalf(Convert.ToSingle(geometry.LayerUV.UVs[++uvIndex]));
                vertIndex++;
                normalIndex++;
                uvIndex++;
            }
            return vertices;
        }

        private Short3[] ConvertFBXTriangles()
        {
            Short3[] triangles = new Short3[geometry.Triangles.Length / 3];

            int triangleIndex = 0;
            for (int i = 0; i != triangles.Length; i++)
            {
                triangles[i] = new Short3();
                triangles[i].S1 = Convert.ToInt16(geometry.Triangles[triangleIndex]);
                triangles[i].S2 = Convert.ToInt16(geometry.Triangles[++triangleIndex]);
                triangles[i].S3 = Convert.ToInt16(~geometry.Triangles[++triangleIndex]);
                triangleIndex++;
            }

            return triangles;
        }
    }
}