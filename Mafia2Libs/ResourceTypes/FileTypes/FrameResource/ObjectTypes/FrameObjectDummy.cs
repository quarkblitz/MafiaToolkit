﻿using Rendering.Factories;
using Rendering.Graphics;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using Utils.Extensions;
using Utils.VorticeUtils;
using Vortice.Mathematics;

namespace ResourceTypes.FrameResource
{
    public class FrameObjectDummy : FrameObjectJoint
    {
        private BoundingBox bounds;

        public BoundingBox Bounds {
            get { return bounds; }
            set { bounds = value; }
        }
        [TypeConverter(typeof(Vector3Converter))]
        public Vector3 BoundaryBoxMinimum
        {
            get { return bounds.Minimum; }
            set { bounds.SetMinimum(value); }
        }
        [TypeConverter(typeof(Vector3Converter))]
        public Vector3 BoundaryBoxMaximum
        {
            get { return bounds.Maximum; }
            set { bounds.SetMaximum(value); }
        }

        public FrameObjectDummy(MemoryStream reader, bool isBigEndian) : base()
        {
            ReadFromFile(reader, isBigEndian);
        }

        public FrameObjectDummy(FrameObjectDummy other) : base(other)
        {
            bounds = other.bounds;
        }

        public FrameObjectDummy() : base()
        {
            bounds = new BoundingBox();
        }

        public override void ReadFromFile(MemoryStream reader, bool isBigEndian)
        {
            base.ReadFromFile(reader, isBigEndian);
            bounds = BoundingBoxExtenders.ReadFromFile(reader, isBigEndian);
        }

        public override void WriteToFile(BinaryWriter writer)
        {
            base.WriteToFile(writer);
            bounds.WriteToFile(writer);
        }

        public override string ToString()
        {
            return name.ToString();
        }

        public override void ConstructRenderable()
        {
            RenderBoundingBox Renderable = RenderableFactory.BuildBoundingBox(Bounds, WorldTransform);
            RenderAdapter = new Rendering.Core.RenderableAdapter();
            RenderAdapter.InitAdaptor(Renderable, this);
        }
    }
}
