﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Windows.Forms;
using Mafia2;
using System.Collections.Generic;

namespace Rendering.Graphics
{
    public class RenderModel
    {
        public struct ModelPart
        {
            public Material Material;
            public ulong MaterialHash;
            public string TextureName;
            public ShaderResourceView Texture;
            public uint StartIndex;
            public uint NumFaces;
        }
        //private string AOTextureName { get; set; }
        public ShaderResourceView AOTexture { get; set; }
        private Buffer VertexBuffer { get; set; }
        private Buffer IndexBuffer { get; set; }
        public ShaderClass.Vertex[] Vertices { get; private set; }
        public RenderBoundingBox BoundingBox { get; private set; }
        //public ModelPart[] ModelParts { get; set; }
        //public ushort[] Indices { get; private set; }

        //new
        public struct LOD
        {
            public ModelPart[] ModelParts { get; set; }
            public ShaderClass.Vertex[] Vertices { get; set; }
            public ushort[] Indices { get; set; }
        }

        public LOD[] LODs { get; private set; }
        public Matrix Transform { get; private set; }
        public bool DoRender { get; set; }

        public RenderModel()
        {
            Transform = Matrix.Identity;
            BoundingBox = new RenderBoundingBox();
        }

        public bool Init(Device device)
        {
            if (!InitBuffer(device))
            {
                MessageBox.Show("unable to init buffer");
                return false;
            }
            if (!LoadTexture(device))
            {
                MessageBox.Show("unable to load texture");
                return false;
            }
            return true;
        }

        public void SetTransform(Vector3 position, Matrix33 rotation)
        {
            Matrix m_trans = Matrix.Identity;
            m_trans[0, 0] = rotation.M00;
            m_trans[0, 1] = rotation.M01;
            m_trans[0, 2] = rotation.M02;
            m_trans[1, 0] = rotation.M10;
            m_trans[1, 1] = rotation.M11;
            m_trans[1, 2] = rotation.M12;
            m_trans[2, 0] = rotation.M20;
            m_trans[2, 1] = rotation.M21;
            m_trans[2, 2] = rotation.M22;
            m_trans[3, 0] = position.X;
            m_trans[3, 1] = position.Y;
            m_trans[3, 2] = position.Z;
            Transform = m_trans;
        }

        public bool ConvertFrameToRenderModel(FrameObjectSingleMesh mesh, FrameGeometry geom, FrameMaterial mats, IndexBuffer[] indexBuffers, VertexBuffer[] vertexBuffers)
        {
            if (mesh == null || geom == null || mats == null || indexBuffers == null || vertexBuffers == null)
                return false;

            SetTransform(mesh.Matrix.Position, mesh.Matrix.Rotation);
            DoRender = true;
            BoundingBox = new RenderBoundingBox();
            BoundingBox.Init(mesh.Boundings);
            LODs = new LOD[geom.NumLods];

            for(int i = 0; i != geom.NumLods; i++)
            {
                LOD lod = new LOD();
                lod.Indices = indexBuffers[i].Data;
                lod.ModelParts = new ModelPart[mats.LodMatCount[i]];

                for (int z = 0; z != mats.Materials[i].Length; z++)
                {
                    lod.ModelParts[z] = new ModelPart();
                    lod.ModelParts[z].NumFaces = (uint)mats.Materials[i][z].NumFaces;
                    lod.ModelParts[z].StartIndex = (uint)mats.Materials[i][z].StartIndex;
                    lod.ModelParts[z].MaterialHash = mats.Materials[i][z].MaterialHash;
                    //Material mat = MaterialsManager.LookupMaterialByHash(lod.ModelParts[z].MaterialHash);
                }

                lod.Vertices = new ShaderClass.Vertex[geom.LOD[i].NumVertsPr];
                int vertexSize;
                Dictionary<VertexFlags, FrameLOD.VertexOffset> vertexOffsets = geom.LOD[i].GetVertexOffsets(out vertexSize);

                for (int x = 0; x != lod.Vertices.Length; x++)
                {
                    ShaderClass.Vertex vertex = new ShaderClass.Vertex();

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Position))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                        vertex.position = VertexTranslator.ReadPositionDataFromVB(vertexBuffers[i].Data, startIndex, geom.DecompressionFactor, geom.DecompressionOffset);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Tangent))
                    {
                        //int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Position].Offset;
                       // vertex.Tangent = VertexTranslator.ReadTangentDataFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.Normals))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.Normals].Offset;
                        vertex.normal = VertexTranslator.ReadNormalDataFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.BlendData))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.BlendData].Offset;
                       // vertex.BlendWeight = VertexTranslator.ReadBlendWeightFromVB(vertexBuffer.Data, startIndex);
                       // vertex.BoneID = VertexTranslator.ReadBlendIDFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.flag_0x80))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords0))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.TexCoords0].Offset;
                        vertex.tex0 = VertexTranslator.ReadTexcoordFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords1))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords1].Offset;
                        //vertex.UVs[1] = VertexTranslator.ReadTexcoordFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords2))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.TexCoords2].Offset;
                        //vertex.UVs[2] = VertexTranslator.ReadTexcoordFromVB(vertexBuffer.Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.TexCoords7))
                    {
                        int startIndex = x * vertexSize + vertexOffsets[VertexFlags.TexCoords7].Offset;
                        vertex.tex7 = VertexTranslator.ReadTexcoordFromVB(vertexBuffers[i].Data, startIndex);
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.flag_0x20000))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.flag_0x40000))
                    {
                        //unknown
                    }

                    if (geom.LOD[i].VertexDeclaration.HasFlag(VertexFlags.DamageGroup))
                    {
                        //int startIndex = v * vertexSize + vertexOffsets[VertexFlags.DamageGroup].Offset;
                        //vertex.DamageGroup = VertexTranslator.ReadDamageGroupFromVB(vertexBuffer.Data, startIndex);
                    }

                    lod.Vertices[x] = vertex;
                }
                LODs[i] = lod;
            }


            return true;
        }
        public void Shutdown()
        {
            ReleaseTextures();
            ShutdownBuffers();
        }
        public void Render(DeviceContext deviceContext)
        {
            RenderBuffers(deviceContext);
        }
        private bool InitBuffer(Device device)
        {
            VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, LODs[0].Vertices);
            IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, LODs[0].Indices);

            BoundingBox.InitBuffer(device);
            return true;
        }
        private bool LoadTexture(Device device)
        {
            TextureClass AOTextureClass = new TextureClass();
            bool result = AOTextureClass.Init(device, "texture.dds");
            AOTexture = AOTextureClass.TextureResource;

            if (!result)
                return false;

            for (int x = 0; x != LODs[0].ModelParts.Length; x++)
            {
                TextureClass Texture = new TextureClass();
                result = Texture.Init(device, LODs[0].ModelParts[x].TextureName);
                LODs[0].ModelParts[x].Texture = Texture.TextureResource;

                if (!result)
                    return false;
            }
            return true;
        }
        private void ReleaseTextures()
        {
            for (int x = 0; x != LODs[0].ModelParts.Length; x++)
            {
                LODs[0].ModelParts[x].Texture?.Dispose();
                LODs[0].ModelParts[x].Texture = null;
            }
            AOTexture?.Dispose();
            AOTexture = null;
            BoundingBox.ReleaseTextures();
        }
        private void ReleaseModel()
        {
            LODs[0].Vertices = null;
            LODs[0].Indices = null;
            BoundingBox.ReleaseModel();
        }
        private void ShutdownBuffers()
        {
            BoundingBox.ShutdownBuffers();
            VertexBuffer?.Dispose();
            VertexBuffer = null;
            IndexBuffer?.Dispose();
            IndexBuffer = null;
        }
        private void RenderBuffers(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<ShaderClass.Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }
}
