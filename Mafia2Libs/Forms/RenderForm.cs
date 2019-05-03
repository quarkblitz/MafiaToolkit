﻿using Rendering.Graphics;
using Rendering.Input;
using SharpDX.Windows;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Mafia2;
using SharpDX;
using ResourceTypes.FrameNameTable;
using ResourceTypes.FrameResource;
using Utils.Settings;
using ResourceTypes.BufferPools;
using Utils.Types;
using Utils.Lang;
using Mafia2Tool.EditorControls;
using Utils.StringHelpers;
using System.Diagnostics;

namespace Mafia2Tool
{
    public partial class D3DForm : Form
    {
        private InputClass Input { get; set; }
        private GraphicsClass Graphics { get; set; }

        private Point mousePos;
        private Point lastMousePos;
        private FileInfo fileLocation;
        private Ray ray;

        private int nameTableLimit = 0;
        private bool isEntryReady = false;

        public D3DForm(FileInfo info)
        {
            InitializeComponent();
            SceneData.ScenePath = info.DirectoryName;
            fileLocation = info;
            SceneData.BuildData();
            PopulateList(info);
            TEMPCameraSpeed.Text = ToolkitSettings.CameraSpeed.ToString();
            KeyPreview = true;
            RenderPanel.Focus();
            StartD3DPanel();
        }

        public void PopulateList(FileInfo info)
        {
            TreeNode tree = SceneData.FrameResource.BuildTree(SceneData.FrameNameTable);
            treeView1.Nodes.Add(tree);
        }

        public void StartD3DPanel()
        {
            Init(RenderPanel.Handle);
            Run();
        }

        public bool Init(IntPtr handle)
        {
            bool result = false;

            if (Input == null)
            {
                Input = new InputClass();
                Input.Init();
            }

            if (Graphics == null)
            {
                Graphics = new GraphicsClass();
                Graphics.PreInit(handle);
                BuildRenderObjects();
                result = Graphics.InitScene();
                UpdateMatrices();
            }
            return result;
        }

        public void Run()
        {
            KeyDown += (s, e) => Input.KeyDown(e.KeyCode);
            KeyUp += (s, e) => Input.KeyUp(e.KeyCode);
            RenderPanel.MouseDown += (s, e) => Input.ButtonDown(e.Button);
            RenderPanel.MouseUp += (s, e) => Input.ButtonUp(e.Button);
            RenderPanel.MouseMove += RenderForm_MouseMove;
            RenderPanel.MouseEnter += RenderPanel_MouseEnter;
            RenderLoop.Run(this, () => { if (!Frame()) Shutdown(); });
        }

        private void RenderPanel_MouseEnter(object sender, EventArgs e)
        {
            RenderPanel.Focus();
        }

        private void RenderForm_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos = new Point(e.Location.X, e.Location.Y);
        }

        private void CullModeButton_Click(object sender, EventArgs e)
        {
            Graphics.ToggleD3DCullMode();
        }

        private void FillModeButton_Click(object sender, EventArgs e)
        {
            Graphics.ToggleD3DFillMode();
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            TreeViewUpdateSelected();
        }

        private void OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeViewUpdateSelected();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            SceneData.CleanData();
            RenderStorageSingleton.Instance.TextureCache.Clear();
            Shutdown();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveChanges();
        }

        public bool Frame()
        {
            if (RenderPanel.Focused)
            {
                if (Input.IsButtonDown(MouseButtons.Right))
                {
                    //Graphics.Camera.UpdateMousePosition(mousePos);
                    var dx = 0.25f * (mousePos.X - lastMousePos.X);
                    var dy = 0.25f * (mousePos.Y - lastMousePos.Y);
                    Graphics.Camera.Pitch(dy);
                    Graphics.Camera.Yaw(dx);
                }
                else if (Input.IsButtonDown(MouseButtons.Left))
                {
                    //broken. Lots of refactoring of the old code to get this working.
                    Pick(mousePos.X, mousePos.Y);
                }

                float speed = Graphics.Timer.FrameTime * ToolkitSettings.CameraSpeed;

                if (Input.IsKeyDown(Keys.A))
                    Graphics.Camera.Position.X += speed;

                if (Input.IsKeyDown(Keys.D))
                    Graphics.Camera.Position.X -= speed;

                if (Input.IsKeyDown(Keys.W))
                    Graphics.Camera.Position.Y += speed;

                if (Input.IsKeyDown(Keys.S))
                    Graphics.Camera.Position.Y -= speed;

                if (Input.IsKeyDown(Keys.Q))
                    Graphics.Camera.Position.Z += speed;

                if (Input.IsKeyDown(Keys.E))
                    Graphics.Camera.Position.Z -= speed;
            }
            lastMousePos = mousePos;
            Graphics.Timer.Frame2();
            Graphics.FPS.Frame();
            Graphics.PickingRayBBox.SetTransform(ray.Position, new Matrix33());
            Graphics.Frame();
            toolStripStatusLabel1.Text = Graphics.Camera.Position.ToString();
            toolStripStatusLabel2.Text = string.Format("{0} {1}", mousePos.X, mousePos.Y);
            toolStripStatusLabel3.Text = string.Format("{0} FPS", Graphics.FPS.FPS);
            return true;
        }

        private void UpdateMatrices()
        {
            FrameObjectBase obj1;

            foreach (TreeNode node in treeView1.Nodes)
            {
                obj1 = (node.Tag as FrameObjectBase);
                TransformMatrix matrix = ((obj1 != null) ? obj1.Matrix : new TransformMatrix());

                if (obj1 != null)
                    UpdateRenderedObjects(matrix, obj1);

                foreach (TreeNode cNode in node.Nodes)
                {
                    UpdateChildRenderNodes(cNode, matrix);
                }
            }
        }
        private void UpdateChildRenderNodes(TreeNode node, TransformMatrix matrix)
        {
            FrameObjectBase obj2 = (node.Tag as FrameObjectBase);

            if (obj2 != null)
                UpdateRenderedObjects(matrix, obj2);

            foreach (TreeNode cNode in node.Nodes)
            {
                matrix = ((obj2 != null) ? obj2.Matrix : new TransformMatrix());
                UpdateChildRenderNodes(cNode, matrix);
            }
        }

        private void UpdateRenderedObjects(TransformMatrix obj1Matrix, FrameObjectBase obj)
        {
            if (Graphics.Assets.ContainsKey(obj.RefID))
                Graphics.Assets[obj.RefID].SetTransform(obj1Matrix.Position + obj.Matrix.Position, obj.Matrix.Rotation);
        }

        private void SaveChanges()
        {
            DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileLocation.FullName, FileMode.Create)))
                {
                    SceneData.FrameResource.WriteToFile(writer);
                }
                using (BinaryWriter writer = new BinaryWriter(File.Open(SceneData.FrameNameTable.FileName, FileMode.Create)))
                {
                    FrameNameTable nameTable = new FrameNameTable();
                    nameTable.FileName = SceneData.FrameNameTable.FileName;
                    nameTable.BuildDataFromResource(SceneData.FrameResource);
                    nameTable.WriteToFile(writer);
                    SceneData.FrameNameTable = nameTable;
                }
                SceneData.IndexBufferPool.WriteToFile();
                SceneData.VertexBufferPool.WriteToFile();
                Console.WriteLine("Saved Changes Succesfully");
            }
        }

        private RenderBoundingBox BuildRenderBounds(FrameObjectDummy dummy)
        {
            RenderBoundingBox dummyBBox = new RenderBoundingBox();
            dummyBBox.SetTransform(dummy.Matrix.Position, dummy.Matrix.Rotation);
            dummyBBox.Init(dummy.Bounds);
            return dummyBBox;
        }

        private RenderBoundingBox BuildRenderBounds(FrameObjectArea area)
        {
            RenderBoundingBox areaBBox = new RenderBoundingBox();
            areaBBox.SetTransform(area.Matrix.Position, area.Matrix.Rotation);
            areaBBox.Init(area.Bounds);
            return areaBBox;
        }

        private RenderModel BuildRenderModel(FrameObjectSingleMesh mesh)
        {
            if (mesh.MaterialIndex == -1 && mesh.MeshIndex == -1)
                return null;

            FrameGeometry geom = SceneData.FrameResource.FrameGeometries[mesh.Refs["Mesh"]];
            FrameMaterial mat = SceneData.FrameResource.FrameMaterials[mesh.Refs["Material"]];
            IndexBuffer[] indexBuffers = new IndexBuffer[geom.LOD.Length];
            VertexBuffer[] vertexBuffers = new VertexBuffer[geom.LOD.Length];

            //we need to retrieve buffers first.
            for (int c = 0; c != geom.LOD.Length; c++)
            {
                indexBuffers[c] = SceneData.IndexBufferPool.GetBuffer(geom.LOD[c].IndexBufferRef.uHash);
                vertexBuffers[c] = SceneData.VertexBufferPool.GetBuffer(geom.LOD[c].VertexBufferRef.uHash);
            }

            RenderModel model = new RenderModel();
            model.ConvertFrameToRenderModel(mesh, geom, mat, indexBuffers, vertexBuffers);
            return model;
        }

        private void BuildRenderObjects()
        {
            Dictionary<int, IRenderer> assets = new Dictionary<int, IRenderer>();

            for (int i = 0; i != SceneData.FrameResource.FrameObjects.Count; i++)
            {
                FrameEntry fObject = (SceneData.FrameResource.FrameObjects.ElementAt(i).Value as FrameEntry);

                if (fObject.GetType() == typeof(FrameObjectSingleMesh) || fObject.GetType() == typeof(FrameObjectModel))
                {
                    FrameObjectSingleMesh mesh = (fObject as FrameObjectSingleMesh);
                    RenderModel model = BuildRenderModel(mesh);

                    if (model == null)
                        continue;

                    assets.Add(fObject.RefID, model);
                }

                if (fObject.GetType() == typeof(FrameObjectArea))
                {
                    FrameObjectArea area = (fObject as FrameObjectArea);
                    assets.Add(fObject.RefID, BuildRenderBounds(area));
                }

                if (fObject.GetType() == typeof(FrameObjectDummy))
                {
                    FrameObjectDummy dummy = (fObject as FrameObjectDummy);
                    assets.Add(fObject.RefID, BuildRenderBounds(dummy));

                }
            }

            if (SceneData.roadMap != null)
            {
                TreeNode node = new TreeNode("RoadMap Data");

                for (int i = 0; i != SceneData.roadMap.data3.Length; i++)
                {
                    ResourceTypes.Navigation.SplineProperties properties = SceneData.roadMap.data3[i];
                    RenderRoad road = new RenderRoad();
                    int generatedID = StringHelpers.RandomGenerator.Next();
                    if (properties.unk3 >= 0 && properties.unk3 < SceneData.roadMap.data1.Length)
                    {
                        road.Init(SceneData.roadMap.data1[properties.unk3].points, properties);
                        assets.Add(generatedID, road);
                    }
                    else
                    {
                        if (properties.unk3 > 4096 && properties.unk3 < 4128)
                        {
                            Console.WriteLine("Found subway {0}", i);
                            road.Init(SceneData.roadMap.data1[properties.unk3 - 4096].points, properties);
                            assets.Add(generatedID, road);
                        }
                        else if (properties.unk3 > 24576 && properties.unk3 < 25332)
                        {
                            Console.WriteLine("Found backroad {0}", i);
                            road.Init(SceneData.roadMap.data1[properties.unk3 - 24576].points, properties);
                            assets.Add(generatedID, road);
                        }
                        else if (properties.unk3 > 16384 && properties.unk3 < 16900)
                        {
                            Console.WriteLine("Found boat {0}", i);
                            road.Init(SceneData.roadMap.data1[properties.unk3 - 16384].points, properties);
                            assets.Add(generatedID, road);
                        }
                        else if (properties.unk3 > 32768 && properties.unk3 < 36864)
                        {
                            Console.WriteLine("Found unknown1 {0}", i);
                            road.Init(SceneData.roadMap.data1[properties.unk3 - 32768].points, properties);
                            assets.Add(generatedID, road);
                        }
                        else if (properties.unk3 > 36864)
                        {
                            Console.WriteLine("Found unknown2 {0}", i);
                            road.Init(SceneData.roadMap.data1[properties.unk3 - 36864].points, properties);
                            assets.Add(generatedID, road);
                        }
                        else
                        {
                            Console.WriteLine("Did not add");
                        }
                    }
                    TreeNode child = new TreeNode(i.ToString());
                    child.Text = "ID: " + i;
                    child.Name = generatedID.ToString();
                    child.Tag = road;
                    node.Nodes.Add(child);
                }

                for (int i = 0; i < SceneData.roadMap.data4.Length; i++)
                {
                    if (SceneData.roadMap.data4[i].boundaries.Length > 0)
                    {
                        Vector3[] extraPoints = new Vector3[SceneData.roadMap.data4[i].boundaries.Length+1];
                        Array.Copy(SceneData.roadMap.data4[i].boundaries, extraPoints, SceneData.roadMap.data4[i].boundaries.Length);
                        extraPoints[extraPoints.Length - 1] = extraPoints[0];
                        RenderLine lineBoundary = new RenderLine();
                        lineBoundary.SetColour(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                        lineBoundary.Init(extraPoints);
                        assets.Add(StringHelpers.RandomGenerator.Next(), lineBoundary);
                    }

                    for (int x = 0; x < SceneData.roadMap.data4[i].splines.Length; x++)
                    {
                        RenderLine line = new RenderLine();
                        line.SetColour(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
                        line.Init(SceneData.roadMap.data4[i].splines[x].path);
                        assets.Add(StringHelpers.RandomGenerator.Next(), line);
                    }
                }
                treeView1.Nodes.Add(node);
            }
            if (SceneData.Collisions != null)
            {
                TreeNode node = new TreeNode("Collision Data");

                for (int i = 0; i != SceneData.Collisions.Placements.Count; i++)
                {
                    ResourceTypes.Collisions.Collision.Placement placement = SceneData.Collisions.Placements[i];
                    RenderStaticCollision collision = new RenderStaticCollision();
                    collision.ConvertCollisionToRender(placement, SceneData.Collisions.NXSData[placement.Hash].Data);

                    bool isAdded = false;
                    int inc = 0;
                    int hash = 0;
                    while(!isAdded)
                    {
                        hash = (int)(placement.Hash);
                        hash += inc;
                        isAdded = !assets.ContainsKey(hash);
                        inc++;
                    }
                    assets.Add(hash, collision);

                    TreeNode child = new TreeNode(hash.ToString());
                    child.Text = "Hash: " + placement.Hash;
                    child.Name = hash.ToString();
                    child.Tag = SceneData.Collisions.NXSData[placement.Hash];
                    node.Nodes.Add(child);
                }

                treeView1.Nodes.Add(node);
            }
            if (SceneData.ATLoader != null)
            {
                for(int i = 0; i != SceneData.ATLoader.paths.Length; i++)
                {
                    ResourceTypes.Navigation.AnimalTrafficLoader.AnimalTrafficPath path = SceneData.ATLoader.paths[i];
                    RenderBoundingBox bbox = new RenderBoundingBox();
                    bbox.SetTransform(new Vector3(0.0f), new Matrix33());
                    bbox.Init(path.bbox);

                    RenderLine line = new RenderLine();
                    line.SetTransform(new Vector3(0.0f), new Matrix33());
                    List<Vector3> points = new List<Vector3>();
                    points.Add(path.bbox.Center);

                    for(int x = 0; x < path.vectors.Length; x++)
                        points.Add(path.vectors[x].vectors[0]);

                    line.SetColour(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
                    line.Init(points.ToArray());
                    assets.Add(StringHelpers.RandomGenerator.Next(), bbox);
                    assets.Add(StringHelpers.RandomGenerator.Next(), line);
                }
            }
            Graphics.InitObjectStack = assets;
        }

        private void TreeViewUpdateSelected()
        {
            if (treeView1.SelectedNode.Tag == null || treeView1.SelectedNode.Tag.GetType() == typeof(FrameHeaderScene))
                return;

            if(treeView1.SelectedNode.Tag is RenderRoad)
            {
                Graphics.BuildSelectedEntry(Convert.ToInt32(treeView1.SelectedNode.Name));
            }
            else if (treeView1.SelectedNode.Tag is ResourceTypes.Collisions.Collision.NXSStruct)
            {
                Graphics.BuildSelectedEntry(Convert.ToInt32(treeView1.SelectedNode.Name));
            }
            else if(treeView1.SelectedNode.Tag is FrameObjectBase)
            {
                UpdateCurrentEntryData(treeView1.SelectedNode.Tag as FrameObjectBase);
            }         
        }

        private void UpdateCurrentEntryData(FrameObjectBase fObject)
        {
            isEntryReady = false;
            CurrentEntry.Text = fObject.Name.String;
            PositionXNumeric.Value = Convert.ToDecimal(fObject.Matrix.Position.X);
            PositionYNumeric.Value = Convert.ToDecimal(fObject.Matrix.Position.Y);
            PositionZNumeric.Value = Convert.ToDecimal(fObject.Matrix.Position.Z);
            RotationXNumeric.Value = Convert.ToDecimal(fObject.Matrix.Rotation.EulerRotation.X);
            RotationYNumeric.Value = Convert.ToDecimal(fObject.Matrix.Rotation.EulerRotation.Y);
            RotationZNumeric.Value = Convert.ToDecimal(fObject.Matrix.Rotation.EulerRotation.Z);
            CurrentEntryType.Text = fObject.GetType().Name;
            Graphics.BuildSelectedEntry(fObject.RefID);
            DebugPropertyGrid.SelectedObject = fObject;
            OnFrameNameTable.Checked = fObject.IsOnFrameTable;
            FrameNameTableFlags.EnumValue = (Enum)Convert.ChangeType(fObject.FrameNameTableFlags, typeof(NameTableFlags));
            isEntryReady = true;
        }

        private void ApplyEntryChanges()
        {
            FrameObjectBase fObject = (treeView1.SelectedNode.Tag as FrameObjectBase);
            fObject.Matrix.Position = new Vector3(Convert.ToSingle(PositionXNumeric.Value), Convert.ToSingle(PositionYNumeric.Value), Convert.ToSingle(PositionZNumeric.Value));
            fObject.Matrix.Rotation.EulerRotation = new Vector3(Convert.ToSingle(RotationXNumeric.Value), Convert.ToSingle(RotationYNumeric.Value), Convert.ToSingle(RotationZNumeric.Value));
            fObject.Matrix.Rotation.UpdateMatrixFromEuler();
            fObject.IsOnFrameTable = OnFrameNameTable.Checked;
            fObject.FrameNameTableFlags = (NameTableFlags)FrameNameTableFlags.GetCurrentValue();
            Graphics.BuildSelectedEntry(fObject.RefID);
            UpdateMatrices();
            ApplyChangesToRenderable(fObject);
        }

        private void ApplyChangesToRenderable(FrameObjectBase obj)
        {
            if (obj is FrameObjectArea)
            {
                FrameObjectArea area = (obj as FrameObjectArea);
                RenderBoundingBox bbox = (Graphics.Assets[obj.RefID] as RenderBoundingBox);
                bbox.Init(area.Bounds);
                Graphics.UpdateObjectStack.Add(obj.RefID, bbox);
            }
        }

        private FrameObjectBase CreateSingleMesh()
        {
            FrameObjectBase mesh = new FrameObjectSingleMesh();

            Model model = new Model();
            model.FrameMesh = (mesh as FrameObjectSingleMesh);

            if (MeshBrowser.ShowDialog() == DialogResult.Cancel)
                return null;

            if (MeshBrowser.FileName.ToLower().EndsWith(".m2t"))
                model.ModelStructure.ReadFromM2T(new BinaryReader(File.Open(MeshBrowser.FileName, FileMode.Open)));
            else if (MeshBrowser.FileName.ToLower().EndsWith(".fbx"))
            {
                if (model.ModelStructure.ReadFromFbx(MeshBrowser.FileName) == -1)
                    return null;
            }

            FrameResourceModelOptions options = new FrameResourceModelOptions();
            options.ShowDialog();

            if (options.type == -1)
                return null;

            bool[] data = options.data;
            options.Dispose();

            //for (int i = 0; i != model.ModelStructure.Lods.Length; i++)
            //{
            //    if (data[0])
            //    {
            //        model.ModelStructure.Lods[i].VertexDeclaration -= VertexFlags.Normals;
            //        model.ModelStructure.Lods[i].VertexDeclaration -= VertexFlags.Tangent;
            //    }

            //    if (data[5])
            //        model.ModelStructure.FlipUVs();
            //}

            mesh.Name.Set(model.ModelStructure.Name);
            model.CreateObjectsFromModel();
            mesh.AddRef(FrameEntryRefTypes.Mesh, model.FrameGeometry.RefID);
            mesh.AddRef(FrameEntryRefTypes.Material, model.FrameMaterial.RefID);
            SceneData.FrameResource.FrameMaterials.Add(model.FrameMaterial.RefID, model.FrameMaterial);
            SceneData.FrameResource.FrameGeometries.Add(model.FrameGeometry.RefID, model.FrameGeometry);

            //Check for existing buffer; if it exists, remove so we can add one later.
            if (SceneData.IndexBufferPool.SearchBuffer(model.IndexBuffers[0].Hash) != null)
                SceneData.IndexBufferPool.RemoveBuffer(model.IndexBuffers[0]);

            //do the same for vertexbuffer pools.
            if (SceneData.VertexBufferPool.SearchBuffer(model.VertexBuffers[0].Hash) != null)
                SceneData.VertexBufferPool.RemoveBuffer(model.VertexBuffers[0]);

            SceneData.IndexBufferPool.AddBuffer(model.IndexBuffers[0]);
            SceneData.VertexBufferPool.AddBuffer(model.VertexBuffers[0]);

            return mesh;
        }

        private void CreateNewEntry(int selected, string name)
        {
            FrameObjectBase frame;

            switch(selected)
            {
                case 0:
                    frame = CreateSingleMesh();

                    if (frame == null)
                        return;
                    break;
                case 1:
                    frame = new FrameObjectFrame();
                    break;
                case 2:
                    frame = new FrameObjectLight();
                    break;
                case 3:
                    frame = new FrameObjectCamera();
                    break;
                case 4:
                    frame = new FrameObjectComponent_U005();
                    break;
                case 5:
                    frame = new FrameObjectSector();
                    break;
                case 6:
                    frame = new FrameObjectDummy();
                    break;
                case 7:
                    frame = new FrameObjectDeflector();
                    break;
                case 8:
                    frame = new FrameObjectArea();
                    break;
                case 9:
                    frame = new FrameObjectTarget();
                    break;
                case 10:
                    throw new NotImplementedException();
                    break;
                case 11:
                    frame = new FrameObjectCollision();
                    break;
                default:
                    frame = new FrameObjectBase();
                    Console.WriteLine("Unknown type selected");
                    break;
            }

            frame.Name.Set(name);
            SceneData.FrameResource.FrameObjects.Add(frame.RefID, frame);
            TreeNode node = new TreeNode(frame.Name.String);
            node.Tag = frame;
            node.Name = frame.RefID.ToString();
            treeView1.Nodes.Add(node);

            if (frame.GetType() == typeof(FrameObjectSingleMesh) || frame.GetType() == typeof(FrameObjectModel))
            {
                FrameObjectSingleMesh mesh = (frame as FrameObjectSingleMesh);
                RenderModel model = BuildRenderModel(mesh);

                Graphics.InitObjectStack.Add(frame.RefID, model);
            }

            if (frame.GetType() == typeof(FrameObjectArea))
            {
                FrameObjectArea area = (frame as FrameObjectArea);
                Graphics.InitObjectStack.Add(frame.RefID, BuildRenderBounds(area));
            }

            if (frame.GetType() == typeof(FrameObjectDummy))
            {
                FrameObjectDummy dummy = (frame as FrameObjectDummy);
                Graphics.InitObjectStack.Add(frame.RefID, BuildRenderBounds(dummy));

            }
        }

        private void Pick(int sx, int sy)
        {
            ray = Graphics.Camera.GetPickingRay(new Vector2(sx, sy), new Vector2(ToolkitSettings.Width, ToolkitSettings.Height));
            //toolStripStatusLabel4.Text = string.Format("{0}, {1}", ray.Position, ray.Direction);
            //FrameObjectSingleMesh selected = null;
            //float seltMin = float.MaxValue;

            foreach (KeyValuePair<int, IRenderer> model in Graphics.Assets)
            {
                var inverseMat = Matrix.Invert(model.Value.Transform);
                ray.Direction = Vector3.TransformNormal(ray.Direction, inverseMat);
                ray.Position = Vector3.TransformCoordinate(ray.Position, inverseMat);
                ray.Direction.Normalize();

                float tmin1 = 0.0f;
                float tmin2 = 0.0f;

                Vector3 minVector = new Vector3(
                model.Value.Transform.M41 + model.Value.BBox.Minimum.X,
                model.Value.Transform.M42 + model.Value.BBox.Minimum.Y,
                model.Value.Transform.M43 + model.Value.BBox.Minimum.Z
                );
                Vector3 maxVector = new Vector3(
                   model.Value.Transform.M41 + model.Value.BBox.Maximum.X,
                   model.Value.Transform.M42 + model.Value.BBox.Maximum.Y,
                   model.Value.Transform.M43 + model.Value.BBox.Maximum.Z
                   );
                BoundingBox tempBox0 = new BoundingBox(minVector, maxVector);
                BoundingBox tempBox1 = model.Value.BBox;

                if (ray.Intersects(ref tempBox0, out tmin1)) { }
                if (ray.Intersects(ref tempBox1, out tmin2)) { }
                Debug.WriteLine(tmin1);
                Debug.WriteLine(tmin2);
            }

            //foreach (KeyValuePair<int, RenderModel> model in Graphics.Models)
            //{
            //    Matrix worldMat = model.Value.Transform;
            //    var invWorld = Matrix.Invert(worldMat);
            //    //ray.Direction = Vector3.TransformNormal(ray.Direction, invWorld);
            //    //ray.Position = Vector3.TransformCoordinate(ray.Position, invWorld);
            //    //ray.Direction.Normalize();

            //    FrameObjectSingleMesh objBase = null;
            //    foreach (KeyValuePair<int, object> obj in SceneData.FrameResource.FrameObjects)
            //    {
            //        if ((obj.Value as FrameObjectBase).RefID == model.Key)
            //            objBase = (obj.Value as FrameObjectSingleMesh);
            //    }

            //    if (objBase == null)
            //        continue;

            //    Vector3 minVector = new Vector3(
            //    model.Value.Transform.M41 + model.Value.BoundingBox.Boundings.Minimum.X,
            //    model.Value.Transform.M42 + model.Value.BoundingBox.Boundings.Minimum.Y,
            //    model.Value.Transform.M43 + model.Value.BoundingBox.Boundings.Minimum.Z
            //    );
            //    Vector3 maxVector = new Vector3(
            //       model.Value.Transform.M41 + model.Value.BoundingBox.Boundings.Maximum.X,
            //       model.Value.Transform.M42 + model.Value.BoundingBox.Boundings.Maximum.Y,
            //       model.Value.Transform.M43 + model.Value.BoundingBox.Boundings.Maximum.Z
            //       );
            //   BoundingBox tempBox0 = new BoundingBox(minVector, maxVector);
            //    BoundingBox tempBox1 = objBase.Boundings;
            //    float tmin;

            //    if (!ray.Intersects(ref tempBox0, out tmin))
            //        continue;

            //    Console.WriteLine("intersect with {0} {1}", objBase.Name.String, tmin);

            //    if (tmin < seltMin)
            //    {
            //        selected = objBase;
            //        seltMin = tmin;
            //    }

            //    //float maxT = float.MaxValue;
            //    //for (var i = 0; i < model.Value.LODs[0].Indices.Length / 3; i++)
            //    //{
            //    //    var v0 = model.Value.Transform.M41 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3]].Position;
            //    //    var v1 = model.Value.Transform.M42 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 1]].Position;
            //    //    var v2 = model.Value.Transform.M43 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 2]].Position;
            //    //    float t = 0;
            //    //    if (!ray.Intersects(ref v0, ref v1, ref v2, out t)) continue;
            //    //    if (!(t < tmin || t < 0)) continue;
            //    //    maxT = t;
            //    //}

            //    //float curTmin = float.MaxValue;
            //    //if (ray.Position.X > tempBox0.Minimum.X && ray.Position.X < tempBox0.Maximum.X)
            //    //{
            //    //    if (ray.Position.Y > tempBox0.Minimum.Y && ray.Position.Y < tempBox0.Maximum.Y)
            //    //    {
            //    //        if (ray.Position.Z > tempBox0.Minimum.Z && ray.Position.Z < tempBox0.Maximum.Z)
            //    //        {

            //    //            for (var i = 0; i < model.Value.LODs[0].Indices.Length / 3; i++)
            //    //            {
            //    //                float tmin2 = float.MaxValue/2;
            //    //                var v0 = model.Value.Transform.M41 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3]].Position;
            //    //                var v1 = model.Value.Transform.M42 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 1]].Position;
            //    //                var v2 = model.Value.Transform.M43 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 2]].Position;
            //    //                float t = 0;
            //    //                if (!ray.Intersects(ref v0, ref v1, ref v2, out t)) continue;
            //    //                // find the closest intersection, exclude intersections behind camera
            //    //                if (!(t < tmin2 || t < 0)) continue;
            //    //                tmin2 = t;
            //    //                if (curTmin < tmin2)
            //    //                {
            //    //                    selected = objBase;
            //    //                }
            //    //            }
            //    //        }
            //    //    }
            //    //}
            //    //ray.Intersects(ref tempBox0, out tmin0);
            //    //ray.Intersects(ref tempBox1, out tmin1);
            //    //Console.WriteLine(tmin0 + " " + tmin1);
            //    //continue;

            //    //Console.WriteLine("Intersection!, " + objBase.Name.String);
            //    //float tmin2 = float.MaxValue;
            //    //for (var i = 0; i < model.Value.LODs[0].Indices.Length / 3; i++)
            //    //{
            //    //    var v0 = model.Value.Transform.M41 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3]].Position;
            //    //    var v1 = model.Value.Transform.M42 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 1]].Position;
            //    //    var v2 = model.Value.Transform.M43 + model.Value.LODs[0].Vertices[model.Value.LODs[0].Indices[i * 3 + 2]].Position;
            //    //    float t = 0;
            //    //    if (!ray.Intersects(ref v0, ref v1, ref v2, out t)) continue;
            //    //    // find the closest intersection, exclude intersections behind camera
            //    //    if (!(t < tmin2 || t < 0)) continue;
            //    //    tmin2 = t;
            //    //}
            //    //if (tmin < tmin2)
            //    //{
            //    //    selected = objBase;
            //    //}
            //}

            //if (selected != null)
            //{
            //    Graphics.BuildSelectedEntry(selected);
            //    Console.WriteLine(selected.Name.String);
            //    UpdateCurrentEntryData(selected);
            //}
        }

        public void Shutdown()
        {
            Graphics?.Shutdown();
            Graphics = null;
            Input = null;
            RenderStorageSingleton.Instance.Shutdown();
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            //FrameObjectBase obj = (treeView1.SelectedNode.Tag as FrameObjectBase);
            //RenderModelView viewer = new RenderModelView(obj.RefID, Graphics.Models[obj.RefID]);
        }

        private void OnPropertyChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label == "RefID")
            {
                TreeNode[] nodes = treeView1.Nodes.Find(e.ChangedItem.Value.ToString(), true);

                if (nodes.Length > 0)
                {
                    int newValue = (int)e.ChangedItem.Value;
                    FrameObjectBase obj = (treeView1.SelectedNode.Tag as FrameObjectBase);
                    int newIndex = 0;
                    string name = "";

                    for(int i = 0; i != SceneData.FrameResource.FrameObjects.Count; i++)
                    {
                        FrameObjectBase frameObj = (SceneData.FrameResource.FrameObjects.ElementAt(i).Value as FrameObjectBase);

                        if (frameObj.RefID == newValue)
                        {
                            newIndex = i;
                            name = frameObj.Name.String;
                        }
                    }

                    if (newIndex == -1)
                    {
                        for (int i = 0; i != SceneData.FrameResource.FrameScenes.Count; i++)
                        {
                            FrameHeaderScene frameObj = SceneData.FrameResource.FrameScenes.ElementAt(i).Value;

                            if (frameObj.RefID == newValue)
                            {
                                newIndex = i;
                                name = frameObj.Name.String;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(name))
                        name = "unknown";

                    //because C# doesn't allow me to get this data for some odd reason, im going to check for it in obj. Why does C# not allow me to see FullLabel in the e var?      
                    if (obj.ParentIndex1.RefID == newValue)
                    {
                        obj.ParentIndex1.Index = newIndex;
                        obj.ParentIndex1.Name = name;
                        obj.SubRef(FrameEntryRefTypes.Parent1);
                        obj.AddRef(FrameEntryRefTypes.Parent1, newValue);
                    }
                    else if (obj.ParentIndex2.RefID == newValue)
                    {
                        obj.ParentIndex2.Index = newIndex;
                        obj.ParentIndex2.Name = name;
                        obj.SubRef(FrameEntryRefTypes.Parent2);
                        obj.AddRef(FrameEntryRefTypes.Parent2, newValue);
                    }
                    treeView1.Nodes.Remove(treeView1.SelectedNode);
                    TreeNode newNode = new TreeNode(obj.ToString());
                    newNode.Tag = obj;
                    newNode.Name = obj.RefID.ToString();
                    nodes[0].Nodes.Add(newNode);
                    treeView1.SelectedNode = newNode;
                }
            }
        }

        private void CameraSpeedUpdate(object sender, EventArgs e)
        {
            float.TryParse(TEMPCameraSpeed.Text, out ToolkitSettings.CameraSpeed);

            if (ToolkitSettings.CameraSpeed == 0.0f)
            {
                ToolkitSettings.CameraSpeed = 0.1f;
                TEMPCameraSpeed.Text = ToolkitSettings.CameraSpeed.ToString();
            }

            ToolkitSettings.WriteKey("CameraSpeed", "ModelViewer", ToolkitSettings.CameraSpeed.ToString());
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;

            if (node.Nodes.Count > 0)
            {
                MessageBox.Show("Cannot delete a node with children!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                FrameEntry obj = node.Tag as FrameEntry;

                if (obj != null)
                {
                    treeView1.Nodes.Remove(node);
                    Graphics.Assets.Remove(obj.RefID);
                    SceneData.FrameResource.FrameObjects.Remove(obj.RefID);

                    if(Graphics.Assets.ContainsKey(obj.RefID))
                        Graphics.Assets.Remove(obj.RefID);
                }
            }

        }

        private void OpenEntryContext(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EntryMenuStrip.Items[0].Visible = false;
            EntryMenuStrip.Items[1].Visible = false;
            EntryMenuStrip.Items[2].Visible = false;
            EntryMenuStrip.Items[3].Visible = false;

            if (treeView1.SelectedNode == null)
                e.Cancel = false;

            if(!e.Cancel)
            {
                EntryMenuStrip.Items[0].Visible = true;
                EntryMenuStrip.Items[1].Visible = true;
                EntryMenuStrip.Items[2].Visible = true;

                if ((treeView1.SelectedNode.Tag != null) && (treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectSingleMesh) || treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectModel) || treeView1.SelectedNode.Tag.GetType() == typeof(ResourceTypes.Collisions.Collision.NXSStruct)))
                    EntryMenuStrip.Items[3].Visible = true;
            }
        }

        private void DuplicateButton_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            FrameObjectBase newEntry = null;

            //is this even needed? hmm.
            if (node.Tag.GetType() == typeof(FrameObjectArea))
            {
                newEntry = new FrameObjectArea((FrameObjectArea)node.Tag);
                FrameObjectArea area = (newEntry as FrameObjectArea);
                Graphics.InitObjectStack.Add(area.RefID, BuildRenderBounds(area));
            }
            else if (node.Tag.GetType() == typeof(FrameObjectCamera))
                newEntry = new FrameObjectCamera((FrameObjectCamera)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectCollision))
                newEntry = new FrameObjectCollision((FrameObjectCollision)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectComponent_U005))
                newEntry = new FrameObjectComponent_U005((FrameObjectComponent_U005)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectDummy))
            {
                newEntry = new FrameObjectDummy((FrameObjectDummy)node.Tag);
                FrameObjectDummy dummy = (newEntry as FrameObjectDummy);
                Graphics.InitObjectStack.Add(dummy.RefID, BuildRenderBounds(dummy));
            }
            else if (node.Tag.GetType() == typeof(FrameObjectDeflector))
                newEntry = new FrameObjectDeflector((FrameObjectDeflector)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectDummy))
                newEntry = new FrameObjectDummy((FrameObjectDummy)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectFrame))
                newEntry = new FrameObjectFrame((FrameObjectFrame)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectJoint))
                newEntry = new FrameObjectJoint((FrameObjectJoint)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectLight))
                newEntry = new FrameObjectLight((FrameObjectLight)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectModel))
            {
                newEntry = new FrameObjectModel((FrameObjectModel)node.Tag);
                FrameObjectSingleMesh mesh = (newEntry as FrameObjectSingleMesh);
                RenderModel model = BuildRenderModel(mesh);
                Graphics.InitObjectStack.Add(mesh.RefID, model);
            }
            else if (node.Tag.GetType() == typeof(FrameObjectSector))
                newEntry = new FrameObjectSector((FrameObjectSector)node.Tag);
            else if (node.Tag.GetType() == typeof(FrameObjectSingleMesh))
            {
                newEntry = new FrameObjectSingleMesh((FrameObjectSingleMesh)node.Tag);
                FrameObjectSingleMesh mesh = (newEntry as FrameObjectSingleMesh);
                RenderModel model = BuildRenderModel(mesh);
                Graphics.InitObjectStack.Add(mesh.RefID, model);
            }
            else if (node.Tag.GetType() == typeof(FrameObjectTarget))
                newEntry = new FrameObjectTarget((FrameObjectTarget)node.Tag);
            else
                newEntry = new FrameObjectBase((FrameObjectBase)node.Tag);

            newEntry.Name.Set(newEntry.Name.String + "_dupe");
            TreeNode tNode = new TreeNode(newEntry.ToString());
            tNode.Tag = newEntry;
            tNode.Name = newEntry.RefID.ToString();
            treeView1.Nodes.Find(newEntry.ParentIndex2.RefID.ToString(), true)[0].Nodes.Add(tNode);
            SceneData.FrameResource.FrameObjects.Add(newEntry.RefID, newEntry);
            UpdateMatrices();
        }

        private void DisableLodButtonOnClick(object sender, EventArgs e)
        {
            //foreach(KeyValuePair<int, object> pair in SceneData.FrameResource.FrameObjects)
            //{
            //    FrameObjectBase obj = (pair.Value as FrameObjectBase);
                
            //    if(obj.FrameNameTableFlags > (NameTableFlags)nameTableLimit)
            //    {
            //        if (Graphics.Assets.ContainsKey(pair.Key))
            //            Graphics.Assets[pair.Key].DoRender = false;
            //    }
            //}
        }

        private void EntryApplyChanges_OnClick(object sender, EventArgs e)
        {
            ApplyEntryChanges();
        }

        private void NameTableFlagValueChanged(object sender, EventArgs e)
        {
            nameTableLimit = int.Parse(NameTableFlagLimit.Text);
        }

        private void EntryField_ValueChanged(object sender, EventArgs e)
        {
            if (isEntryReady)
                ApplyEntryChanges();
        }

        private void Export3DButton_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode.Tag.GetType() == typeof(ResourceTypes.Collisions.Collision.NXSStruct))
                ExportCollision(treeView1.SelectedNode.Tag as ResourceTypes.Collisions.Collision.NXSStruct);
            else
                Export3DFrame(treeView1.SelectedNode.Tag);
        }

        private void ExportCollision(ResourceTypes.Collisions.Collision.NXSStruct data)
        {
            M2TStructure structure = new M2TStructure();
            structure.BuildCollision(data, treeView1.SelectedNode.Name);
            structure.ExportCollisionToM2T(treeView1.SelectedNode.Name);
            structure.ExportToFbx("Collisions/", false);
        }
        private void Export3DFrame(object tag)
        {
            FrameObjectSingleMesh mesh = (treeView1.SelectedNode.Tag as FrameObjectSingleMesh);
            FrameGeometry geom = SceneData.FrameResource.FrameGeometries[mesh.Refs["Mesh"]];
            FrameMaterial mat = SceneData.FrameResource.FrameMaterials[mesh.Refs["Material"]];
            IndexBuffer[] indexBuffers = new IndexBuffer[geom.LOD.Length];
            VertexBuffer[] vertexBuffers = new VertexBuffer[geom.LOD.Length];

            //we need to retrieve buffers first.
            for (int c = 0; c != geom.LOD.Length; c++)
            {
                indexBuffers[c] = SceneData.IndexBufferPool.GetBuffer(geom.LOD[c].IndexBufferRef.uHash);
                vertexBuffers[c] = SceneData.VertexBufferPool.GetBuffer(geom.LOD[c].VertexBufferRef.uHash);
            }

            Model newModel = new Model(mesh, indexBuffers, vertexBuffers, geom, mat);

            for (int c = 0; c != newModel.ModelStructure.Lods.Length; c++)
            {
                newModel.ModelStructure.ExportToM2T(ToolkitSettings.ExportPath + "\\");
                switch (ToolkitSettings.Format)
                {
                    case 0:
                        newModel.ModelStructure.ExportToFbx(ToolkitSettings.ExportPath + "\\", false);
                        break;
                    case 1:
                        newModel.ModelStructure.ExportToFbx(ToolkitSettings.ExportPath + "\\", true);
                        break;
                    case 2:
                        newModel.ModelStructure.ExportToM2T(ToolkitSettings.ExportPath + "\\");
                        break;
                    default:
                        break;
                }
            }
        }

        private void ToggleCollisionsOnClick(object sender, EventArgs e)
        {
            foreach(KeyValuePair<int, IRenderer> obj in Graphics.Assets)
            {
                if (obj.Value.GetType() == typeof(RenderStaticCollision))
                    obj.Value.DoRender = !obj.Value.DoRender;
            }
        }

        private void ToggleModelOnClick(object sender, EventArgs e)
        {
            foreach (KeyValuePair<int, IRenderer> obj in Graphics.Assets)
            {
                if (obj.Value.GetType() == typeof(RenderModel))
                    obj.Value.DoRender = !obj.Value.DoRender;
            }
        }

        private void AddButtonOnClick(object sender, EventArgs e)
        {
            NewObjectForm form = new NewObjectForm(true);
            form.SetLabel(Language.GetString("$QUESTION_FRADD"));
            form.LoadOption(new FrameResourceAddOption());
            form.ShowDialog();

            int selection;

            if (form.type != -1)
                selection = (form.control as FrameResourceAddOption).GetSelectedType();
            else return;

            CreateNewEntry(selection, form.GetInputText());
        }
    }
}
