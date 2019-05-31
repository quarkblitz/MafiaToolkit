﻿using WeifenLuo.WinFormsUI.Docking;
using System.Windows.Forms;
using ResourceTypes.FrameResource;
using SharpDX;

namespace Forms.Docking
{
    public partial class DockSceneTree : DockContent
    {
        public DockSceneTree()
        {
            InitializeComponent();
        }

        public void AddToTree(TreeNode newNode)
        {
            ApplyImageIndex(newNode);
            RecurseChildren(newNode);
            treeView1.Nodes.Add(newNode);
        }

        public void AddToTree(TreeNode newNode, TreeNode parentNode)
        {
            ApplyImageIndex(newNode);
            RecurseChildren(newNode);
            parentNode.Nodes.Add(newNode);
            treeView1.SelectedNode = newNode;
        }

        private void RecurseChildren(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                ApplyImageIndex(child);
                RecurseChildren(child);
            }
        }

        private void ApplyImageIndex(TreeNode node)
        {
            if (node.Tag == null)
            {
                node.ImageIndex = 7;
                return;
            }

            if (node.Tag.GetType() == typeof(FrameObjectJoint))
                node.SelectedImageIndex = node.ImageIndex = 7;
            else if (node.Tag.GetType() == typeof(FrameObjectSingleMesh))
                node.SelectedImageIndex = node.ImageIndex = 6;
            else if (node.Tag.GetType() == typeof(FrameObjectFrame))
                node.SelectedImageIndex = node.ImageIndex = 0;
            else if (node.Tag.GetType() == typeof(FrameObjectLight))
                node.SelectedImageIndex = node.ImageIndex = 5;
            else if (node.Tag.GetType() == typeof(FrameObjectCamera))
                node.SelectedImageIndex = node.ImageIndex = 2;
            else if (node.Tag.GetType() == typeof(FrameObjectComponent_U005))
                node.SelectedImageIndex = node.ImageIndex = 7;
            else if (node.Tag.GetType() == typeof(FrameObjectSector))
                node.SelectedImageIndex = node.ImageIndex = 7;
            else if (node.Tag.GetType() == typeof(FrameObjectDummy))
                node.SelectedImageIndex = node.ImageIndex = 10;
            else if (node.Tag.GetType() == typeof(FrameObjectDeflector))
                node.SelectedImageIndex = node.ImageIndex = 7;
            else if (node.Tag.GetType() == typeof(FrameObjectArea))
                node.SelectedImageIndex = node.ImageIndex = 1;
            else if (node.Tag.GetType() == typeof(FrameObjectTarget))
                node.SelectedImageIndex = node.ImageIndex = 7;
            else if (node.Tag.GetType() == typeof(FrameObjectModel))
                node.SelectedImageIndex = node.ImageIndex = 9;
            else if (node.Tag.GetType() == typeof(FrameObjectCollision))
                node.SelectedImageIndex = node.ImageIndex = 3;
            else if (node.Tag.GetType() == typeof(ResourceTypes.Collisions.Collision.Placement))
                node.SelectedImageIndex = node.ImageIndex = 4;
            else if (node.Tag.GetType() == typeof(FrameHeaderScene))
                node.SelectedImageIndex = node.ImageIndex = 8;
            else
                node.SelectedImageIndex = node.ImageIndex = 7;
        }

        private void OpenEntryContext(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EntryMenuStrip.Items[0].Visible = false;
            EntryMenuStrip.Items[1].Visible = false;
            EntryMenuStrip.Items[2].Visible = false;
            EntryMenuStrip.Items[3].Visible = false;

            if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag != null)
            {

                EntryMenuStrip.Items[1].Visible = true;
                EntryMenuStrip.Items[2].Visible = true;

                if (treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectBase) || treeView1.SelectedNode.Tag.GetType() == typeof(ResourceTypes.Collisions.Collision.Placement) || treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectJoint))
                {
                    EntryMenuStrip.Items[0].Visible = true;
                }
                if ((treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectSingleMesh) || treeView1.SelectedNode.Tag.GetType() == typeof(FrameObjectModel) || treeView1.SelectedNode.Tag.GetType() == typeof(ResourceTypes.Collisions.Collision.Placement)))
                {
                    EntryMenuStrip.Items[0].Visible = true;
                    EntryMenuStrip.Items[3].Visible = true;
                }
            }
        }

        public Vector3 JumpToHelper()
        {
            object data = treeView1.SelectedNode.Tag;

            if(data.GetType() == typeof(FrameObjectBase) || data.GetType() == typeof(FrameObjectJoint) || data.GetType() == typeof(FrameObjectSingleMesh))
                return (data as FrameObjectBase).Matrix.Position;

            if(data.GetType() == typeof(ResourceTypes.Collisions.Collision.Placement))
                return (data as ResourceTypes.Collisions.Collision.Placement).Position;

            return new Vector3(0, 0, 0);
        }
    }
}