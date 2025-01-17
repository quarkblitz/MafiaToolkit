﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using Utils.Extensions;
using Utils.VorticeUtils;
//roadmap research
//https://media.discordapp.net/attachments/464158725079564303/468180499806945310/unknown.png?width=1202&height=676
//https://media.discordapp.net/attachments/464158725079564303/468180681646931969/unknown.png?width=1442&height=474

//green = main road
//blue = parking
//yellow = optional road, the AI knows its there, but not direct.
//281
//311
//317

//195
//198
//199
//    1 = 
//2 = 
//4 = 
//8 = 
//16 = 
//32 = main road?
//64 = highway?

namespace ResourceTypes.Navigation
{
    public struct SplineDefinition
    {
        //12 bytes max!
        private uint offset;
        private ushort numSplines1; //should be equal to unk2
        private ushort numSplines2;
        private float length;
        private Vector3[] points;

        private SplineProperties toward;
        private SplineProperties backward;
        private bool hasToward;
        private bool hasBackward;
        private int idxOffset;

        public uint Offset {
            get { return offset; }
            set { offset = value; }
        }
        public ushort NumSplines1 {
            get { return numSplines1; }
            set { numSplines1 = value; }
        }
        public ushort NumSplines2 {
            get { return numSplines2; }
            set { numSplines2 = value; }
        }
        public float Length {
            get { return length; }
            set { length = value; }
        }
        public Vector3[] Points {
            get { return points; }
            set { points = value; }
        }
        public SplineProperties Toward {
            get { return toward; }
            set { toward = value; }
        }
        public SplineProperties Backward {
            get { return backward; }
            set { backward = value; }
        }
        public bool HasToward {
            get { return hasToward; }
            set { hasToward = value; }
        }
        public bool HasBackward {
            get { return hasBackward; }
            set { hasBackward = value; }
        }
        public int IndexOffset {
            get { return idxOffset; }
            set { idxOffset = value; }
        }

        public void SetSplineData(SplineProperties data, int offset)
        {
            if (data.Flags.HasFlag(RoadFlags.BackwardDirection))
            {
                backward = data;
                hasBackward = true;
            }
            else
            {
                toward = data;
                hasToward = true;
            }

            idxOffset = offset;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", NumSplines1, NumSplines2, length);
        }
    }

    public class SplineProperties
    {
        private ushort laneIDX0;
        private ushort laneIDX1;
        private uint offset0;
        private ushort laneSize0;
        private ushort laneSize1;
        private uint offset1;
        private ushort rangeSize0;
        private ushort rangeSize1;
        private RoadFlags flags;
        private ushort splineIDX;
        private LaneProperties[] lanes;
        private RangeProperties[] ranges;

        public ushort LaneIDX0 {
            get { return laneIDX0; }
            set { laneIDX0 = value; }
        }
        public ushort LaneIDX1 {
            get { return laneIDX1; }
            set { laneIDX1 = value; }
        }
        [Browsable(false)]
        public uint Offset0 {
            get { return offset0; }
            set { offset0 = value; }
        }
        [Browsable(false)]
        public ushort LaneSize0 {
            get { return laneSize0; }
            set { laneSize0 = value; }
        }
        [Browsable(false)]
        public ushort LaneSize1 {
            get { return laneSize1; }
            set { laneSize1 = value; }
        }
        [Browsable(false)]
        public uint Offset1 {
            get { return offset1; }
            set { offset1 = value; }
        }
        [Browsable(false)]
        public ushort RangeSize0 {
            get { return rangeSize0; }
            set { rangeSize0 = value; }
        }
        [Browsable(false)]
        public ushort RangeSize1 {
            get { return rangeSize1; }
            set { rangeSize1 = value; }
        }

        [Editor(typeof(FlagEnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public RoadFlags Flags {
            get { return flags; }
            set { flags = value; }
        }
        [ReadOnly(true)]
        public ushort SplineIDX {
            get { return splineIDX; }
            set { splineIDX = value; }
        }
        public LaneProperties[] Lanes {
            get { return lanes; }
            set { lanes = value; }
        }
        public RangeProperties[] Ranges {
            get { return ranges; }
            set { ranges = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", laneIDX0, laneIDX1, laneSize0, rangeSize0, flags, splineIDX);
        }
    }

    public class LaneProperties
    {
        //16 bytes
        private float width;
        private LaneTypes flags;
        private ushort unk03;
        private uint rangeOffset;
        private ushort rangeSize0;
        private ushort rangeSize1;
        private RangeProperties[] ranges; 

        public float Width {
            get { return width; }
            set { width = value; }
        }

        [Editor(typeof(FlagEnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public LaneTypes Flags {
            get { return flags; }
            set { flags = value; }
        }
        public ushort Unk03 {
            get { return unk03; }
            set { unk03 = value; }
        }
        [Browsable(false)]
        public uint RangeOffset {
            get { return rangeOffset; }
            set { rangeOffset = value; }
        }

        [Browsable(false)]
        public ushort RangeSize0 {
            get { return rangeSize0; }
            set { rangeSize0 = value; }
        }

        [Browsable(false)]
        public ushort RangeSize1 {
            get { return rangeSize1; }
            set { rangeSize1 = value; }
        }

        public RangeProperties[] Ranges {
            get { return ranges; }
            set { ranges = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", width, flags, unk03);
        }
    }

    public class RangeProperties
    {
        //16 bytes
        private float unk01;
        private float unk02;
        private short unk03;
        private short unk04;
        private float unk05;

        public float Unk01 {
            get { return unk01; }
            set { unk01 = value; }
        }
        public float Unk02 {
            get { return unk02; }
            set { unk02 = value; }
        }
        public short Unk03 {
            get { return unk03; }
            set { unk03 = value; }
        }
        public short Unk04 {
            get { return unk04; }
            set { unk04 = value; }
        }
        public float Unk05 {
            get { return unk05; }
            set { unk05 = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", unk01, unk02, unk03, unk04, unk05);
        }
    }

    public class JunctionDefinition
    {
        Vector3 position;
        public uint offset0;
        public short junctionSize0;
        public short junctionSize1;
        public uint offset1;
        public short boundarySize0;
        public short boundarySize1;
        int junctionIdx;
        public uint offset2;
        short unk5;
        short unk6;
        Vector3[] boundaries; //lines, not a box.
        JunctionSpline[] splines;
        unkStruct2Sect2 dataSet2;

        public Vector3 Position {
            get { return position; }
            set { position = value; }
        }
        public short Unk5 {
            get { return unk5; }
            set { unk5 = value; }
        }
        public short Unk6 {
            get { return unk6; }
            set { unk6 = value; }
        }
        public Vector3[] Boundaries {
            get { return boundaries; }
            set { boundaries = value; }
        }
        public JunctionSpline[] Splines {
            get { return splines; }
            set { splines = value; }
        }
        public unkStruct2Sect2 DataSet2 {
            get { return dataSet2; }
            set { dataSet2 = value; }
        }
        public int JunctionIDX {
            get { return junctionIdx; }
            set { junctionIdx = value; }
        }
        [Browsable(false)]
        public uint Offset0 {
            get { return offset0; }
            set { offset0 = value; }
        }
        [Browsable(false)]
        public uint Offset1 {
            get { return offset1; }
            set { offset1 = value; }
        }
        [Browsable(false)]
        public uint Offset2 {
            get { return offset2; }
            set { offset2 = value; }
        }

        public JunctionDefinition()
        {
            dataSet2 = new unkStruct2Sect2();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", junctionIdx, unk5, unk6);
        }
    }

    public class JunctionSpline
    {
        short unk0;
        short unk1;
        short unk2;
        short unk3;
        short unk4;
        short unk5;
        private uint offset0;
        private short pathSize0;
        private short pathSize1;
        private float length;
        Vector3[] path;

        public short Unk0 {
            get { return unk0; }
            set { unk0 = value; }
        }
        public short Unk1 {
            get { return unk1; }
            set { unk1 = value; }
        }
        public short Unk2 {
            get { return unk2; }
            set { unk2 = value; }
        }
        public short Unk3 {
            get { return unk3; }
            set { unk3 = value; }
        }
        public short Unk4 {
            get { return unk4; }
            set { unk4 = value; }
        }
        public short Unk5 {
            get { return unk5; }
            set { unk5 = value; }
        }
        [Browsable(false)]
        public uint Offset0 {
            get { return offset0; }
            set { offset0 = value; }
        }
        [Browsable(false)]
        public short PathSize0 {
            get { return pathSize0; }
            set { pathSize0 = value; }
        }
        [Browsable(false)]
        public short PathSize1 {
            get { return pathSize1; }
            set { pathSize1 = value; }
        }
        [Browsable(false)]
        public float Length {
            get { return length; }
            set { length = value; }
        }
        public Vector3[] Path {
            get { return path; }
            set { path = value; }
        }

        public JunctionSpline()
        {
            path = new Vector3[pathSize1];
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", unk0, unk1, unk2, unk3, unk4, unk5, unk4, unk5);
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class unkStruct2Sect2
    {
        int unk0;
        uint offset0;
        short unk1;
        short unk2;
        int unk3;
        uint offset1;
        short unk4;
        short unk5;
        short unk6;
        short unk7;
        short unk8;
        short unk9;
        ushort[] unk10;
        ushort[] unk3Bytes;

        public int Unk0 {
            get { return unk0; }
            set { unk0 = value; }
        }
        [Browsable(false)]
        public uint Offset0 {
            get { return offset0; }
            set { offset0 = value; }
        }
        public short Unk1 {
            get { return unk1; }
            set { unk1 = value; }
        }
        public short Unk2 {
            get { return unk2; }
            set { unk2 = value; }
        }
        public int Unk3 {
            get { return unk3; }
            set { unk3 = value; }
        }
        [Browsable(false)]
        public uint Offset1 {
            get { return offset1; }
            set { offset1 = value; }
        }
        public short Unk4 {
            get { return unk4; }
            set { unk4 = value; }
        }
        public short Unk5 {
            get { return unk5; }
            set { unk5 = value; }
        }
        public short Unk6 {
            get { return unk6; }
            set { unk6 = value; }
        }
        public short Unk7 {
            get { return unk7; }
            set { unk7 = value; }
        }
        public short Unk8 {
            get { return unk8; }
            set { unk8 = value; }
        }
        public short Unk9 {
            get { return unk9; }
            set { unk9 = value; }
        }
        public ushort[] Unk10 {
            get { return unk10; }
            set { unk10 = value; }
        }
        public ushort[] Unk3Bytes {
            get { return unk3Bytes; }
            set { unk3Bytes = value; }
        }
    }


    //size: 4
    public class unkStruct3
    {
        ushort unk0;
        ushort unk1;

        public ushort Unk0 {
            get { return unk0; }
            set { unk0 = value; }
        }
        public ushort Unk1 {
            get { return unk1; }
            set { unk1 = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", unk0, unk1);
        }
    }

    public class Roadmap
    {
        public int magic;
        public ushort splineCount;
        public uint splineOffset;
        public ushort splinePropertiesCount;
        public uint splinePropertiesOffset;
        public ushort junctionPropertiesCount;
        public uint junctionPropertiesOffset;
        public ushort unkDataSet3Count;
        public uint unkDataSet3Offset;
        public ushort unkDataSet4Count;
        public uint unkDataSet4Offset;
        public ushort unkGPSSize;
        public uint unkDataSet5Offset;
        public ushort unkDataSet6Count;

        public SplineDefinition[] splines;
        public SplineProperties[] splineData;
        public JunctionDefinition[] junctionData;
        public ushort[] unkSet3;
        public unkStruct3[] unkSet4;
        public ushort[] unkGPS;
        public ushort[] unkSet6;

        private FileInfo info;

        public ushort[] UnkSet3 {
            get { return unkSet3; }
            set { unkSet3 = value; }
        }
        public unkStruct3[] UnkSet4 {
            get { return unkSet4; }
            set { unkSet4 = value; }
        }
        public ushort[] UnkGPS {
            get { return unkGPS; }
            set { unkGPS = value; }
        }
        public ushort[] UnkSet6 {
            get { return unkSet6; }
            set { unkSet6 = value; }
        }

        public Roadmap(FileInfo info)
        {
            this.info = info;
            using (BinaryReader reader = new BinaryReader(File.Open(info.FullName, FileMode.Open)))
            {
                ReadFromFile(reader);
            }
            WriteToFile();
        }

        public void ReadFromFile(BinaryReader reader)
        {
            magic = reader.ReadInt32();
            splineCount = reader.ReadUInt16();
            short splineCount2 = reader.ReadInt16();
            splineOffset = reader.ReadInt24();
            reader.ReadByte();
            splinePropertiesCount = reader.ReadUInt16();
            short splinePropertiesCount2 = reader.ReadInt16();
            splinePropertiesOffset = reader.ReadInt24();
            reader.ReadByte();
            junctionPropertiesCount = reader.ReadUInt16();
            short junctionPropertiesCount2 = reader.ReadInt16();
            junctionPropertiesOffset = reader.ReadInt24();
            reader.ReadByte();
            unkDataSet3Count = reader.ReadUInt16();
            short unkDataSet3Count2 = reader.ReadInt16();
            unkDataSet3Offset = reader.ReadInt24();
            reader.ReadByte();
            unkDataSet4Count = reader.ReadUInt16();
            short unkDataSet4Count2 = reader.ReadInt16();
            unkDataSet4Offset = reader.ReadInt24();
            reader.ReadByte();
            unkGPSSize = reader.ReadUInt16();
            short unkDataSet5Count2 = reader.ReadInt16();
            unkDataSet5Offset = reader.ReadInt24();
            reader.ReadByte();
            unkDataSet6Count = reader.ReadUInt16();
            short unkDataSet6Count2 = reader.ReadInt16();

            int count1 = reader.ReadInt32();
            splines = new SplineDefinition[splineCount];

            for (int i = 0; i != splineCount; i++)
            {
                SplineDefinition data = new SplineDefinition();
                data.Offset = reader.ReadInt24();
                reader.ReadByte();
                data.NumSplines1 = reader.ReadUInt16();
                data.NumSplines2 = reader.ReadUInt16();
                data.Length = reader.ReadSingle();
                splines[i] = data;
            }

            for (int i = 0; i != splineCount; i++)
            {
                SplineDefinition data = splines[i];
                data.Points = new Vector3[splines[i].NumSplines1];
                reader.BaseStream.Position = splines[i].Offset - 4;

                for (int y = 0; y != splines[i].NumSplines1; y++)
                {
                    data.Points[y] = Vector3Utils.ReadFromFile(reader);
                }

                splines[i] = data;
            }

            splineData = new SplineProperties[splinePropertiesCount];
            for (int i = 0; i != splineData.Length; i++)
            {
                SplineProperties data = new SplineProperties();
                data.LaneIDX0 = reader.ReadUInt16();
                data.LaneIDX1 = reader.ReadUInt16();
                data.Offset0 = reader.ReadInt24();
                reader.ReadByte();
                data.LaneSize0 = reader.ReadUInt16();
                data.LaneSize1 = reader.ReadUInt16();
                data.Offset1 = reader.ReadInt24();
                reader.ReadByte();
                data.RangeSize0 = reader.ReadUInt16();
                data.RangeSize1 = reader.ReadUInt16();
                data.Flags = (RoadFlags)reader.ReadInt16();
                data.SplineIDX = reader.ReadUInt16();
                splineData[i] = data;
            }

            for (int i = 0; i != splinePropertiesCount; i++)
            {
                SplineProperties data = splineData[i];
                data.Lanes = new LaneProperties[data.LaneSize1];

                for (int y = 0; y != data.LaneSize1; y++)
                {
                    LaneProperties sect = new LaneProperties();
                    sect.Width = reader.ReadSingle();
                    sect.Flags = (LaneTypes)reader.ReadUInt16();
                    sect.Unk03 = reader.ReadUInt16();
                    sect.RangeOffset = reader.ReadInt24();
                    reader.ReadByte();
                    sect.RangeSize0 = reader.ReadUInt16();
                    sect.RangeSize1 = reader.ReadUInt16();

                    data.Lanes[y] = sect;
                }

                for (int y = 0; y != data.LaneSize1; y++)
                {
                    if (data.Lanes[y].RangeSize0 > 0)
                    {
                        data.Lanes[y].Ranges = new RangeProperties[data.Lanes[y].RangeSize0];
                    }

                    for(int x = 0; x != data.Lanes[y].RangeSize0; x++)
                    {
                        RangeProperties sect = new RangeProperties();
                        sect.Unk01 = reader.ReadSingle();
                        sect.Unk02 = reader.ReadSingle();
                        sect.Unk03 = reader.ReadInt16();
                        sect.Unk04 = reader.ReadInt16();
                        sect.Unk05 = reader.ReadSingle();
                        data.Lanes[y].Ranges[x] = sect;
                    }
                }

                data.Ranges = new RangeProperties[data.RangeSize0];

                for (int y = 0; y != data.RangeSize0; y++)
                {
                    RangeProperties sect = new RangeProperties();
                    sect.Unk01 = reader.ReadSingle();
                    sect.Unk02 = reader.ReadSingle();
                    sect.Unk03 = reader.ReadInt16();
                    sect.Unk04 = reader.ReadInt16();
                    sect.Unk05 = reader.ReadSingle();
                    data.Ranges[y] = sect;
                }

                splineData[i] = data;

                //4095 boat
                if (data.SplineIDX > 4096 && data.SplineIDX < 4128)
                {
                    splines[data.SplineIDX - 4096].SetSplineData(data, 4096);
                }
                else if (data.SplineIDX > 24576 && data.SplineIDX < 25332)
                {
                    splines[data.SplineIDX - 24576].SetSplineData(data, 24576);
                }
                else if (data.SplineIDX > 16384 && data.SplineIDX < 16900)
                {
                    splines[data.SplineIDX - 16384].SetSplineData(data, 16384);
                }
                else if (data.SplineIDX > 32768 && data.SplineIDX < 36864)
                {
                    splines[data.SplineIDX - 32768].SetSplineData(data, 32768);
                }
                else if (data.SplineIDX > 36864)
                {
                    splines[data.SplineIDX - 36864].SetSplineData(data, 36864);
                }
                else
                {
                    splines[data.SplineIDX].SetSplineData(data, 0);
                }
            }

            junctionData = new JunctionDefinition[junctionPropertiesCount];
            for (int i = 0; i != junctionPropertiesCount; i++)
            {
                JunctionDefinition data = new JunctionDefinition();
                data.Position = Vector3Utils.ReadFromFile(reader);
                data.offset0 = reader.ReadInt24();
                reader.ReadByte();
                data.junctionSize0 = reader.ReadInt16();
                data.junctionSize1 = reader.ReadInt16();
                data.offset1 = reader.ReadInt24();
                reader.ReadByte();
                data.boundarySize0 = reader.ReadInt16();
                data.boundarySize1 = reader.ReadInt16();
                data.JunctionIDX = reader.ReadInt32();
                data.offset2 = reader.ReadInt24();
                reader.ReadByte();
                data.Unk5 = reader.ReadInt16();
                data.Unk6 = reader.ReadInt16();
                junctionData[i] = data;
            }

            for (int i = 0; i != junctionPropertiesCount; i++)
            {
                junctionData[i].Splines = new JunctionSpline[junctionData[i].junctionSize0];

                for (int y = 0; y != junctionData[i].junctionSize0; y++)
                {
                    JunctionSpline data4Sect = new JunctionSpline();
                    data4Sect.Unk0 = reader.ReadInt16();
                    data4Sect.Unk1 = reader.ReadInt16();
                    data4Sect.Unk2 = reader.ReadInt16();
                    data4Sect.Unk3 = reader.ReadInt16();
                    data4Sect.Unk4 = reader.ReadInt16();
                    data4Sect.Unk5 = reader.ReadInt16();
                    data4Sect.Offset0 = reader.ReadInt24();
                    reader.ReadByte();
                    data4Sect.PathSize0 = reader.ReadInt16();
                    data4Sect.PathSize1 = reader.ReadInt16();
                    data4Sect.Length = reader.ReadSingle();
                    junctionData[i].Splines[y] = data4Sect;
                }
                for (int y = 0; y != junctionData[i].junctionSize0; y++)
                {
                    junctionData[i].Splines[y].Path = new Vector3[junctionData[i].Splines[y].PathSize0];

                    for (int z = 0; z != junctionData[i].Splines[y].PathSize0; z++)
                    {
                        junctionData[i].Splines[y].Path[z] = Vector3Utils.ReadFromFile(reader);
                    }
                }

                junctionData[i].Boundaries = new Vector3[junctionData[i].boundarySize0];
                for (int y = 0; y != junctionData[i].boundarySize0; y++)
                {
                    junctionData[i].Boundaries[y] = Vector3Utils.ReadFromFile(reader);
                }

                if (junctionData[i].Unk5 >= 2)
                {
                    unkStruct2Sect2 dataSet = new unkStruct2Sect2();
                    dataSet = new unkStruct2Sect2();
                    dataSet.Unk0 = reader.ReadInt32();
                    dataSet.Offset0 = reader.ReadInt24();
                    reader.ReadByte();
                    dataSet.Unk1 = reader.ReadInt16();
                    dataSet.Unk2 = reader.ReadInt16();
                    dataSet.Unk3 = reader.ReadInt32();
                    dataSet.Offset1 = reader.ReadInt24();
                    reader.ReadByte();
                    dataSet.Unk4 = reader.ReadInt16();
                    dataSet.Unk5 = reader.ReadInt16();
                    dataSet.Unk6 = reader.ReadInt16();
                    dataSet.Unk7 = reader.ReadInt16();
                    dataSet.Unk8 = reader.ReadInt16();
                    dataSet.Unk9 = reader.ReadInt16();

                    //this could actually be collection of ints.
                    dataSet.Unk10 = new ushort[8];

                    if (dataSet.Unk1 > 2 && dataSet.Unk2 > 2)
                    { 
                        if (dataSet.Unk1 == 4)
                        {
                            dataSet.Unk10[0] = reader.ReadUInt16();
                            dataSet.Unk10[1] = reader.ReadUInt16();
                            dataSet.Unk10[2] = 0;
                            dataSet.Unk10[3] = 0;
                        }
                        else
                        {
                            dataSet.Unk10[0] = reader.ReadUInt16();
                            dataSet.Unk10[1] = reader.ReadUInt16();
                            dataSet.Unk10[2] = reader.ReadUInt16();
                            dataSet.Unk10[3] = reader.ReadUInt16();
                        }
                    }

                    if (junctionData[i].Unk5 == 3)
                    {
                        dataSet.Unk3Bytes = new ushort[8];
                        for(int x = 0; x < 8; x++)
                        {
                            dataSet.Unk3Bytes[x] = reader.ReadUInt16();
                        }
                    }

                    junctionData[i].DataSet2 = dataSet;
                }
            }

            unkSet3 = new ushort[unkDataSet3Count];
            unkSet4 = new unkStruct3[unkDataSet4Count];
            unkGPS = new ushort[unkGPSSize];
            unkSet6 = new ushort[unkDataSet6Count];

            for (int i = 0; i < unkDataSet3Count; i++)
            {
                unkSet3[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < unkDataSet4Count; i++)
            {
                unkSet4[i] = new unkStruct3();
                unkSet4[i].Unk0 = reader.ReadUInt16();
                unkSet4[i].Unk1 = reader.ReadUInt16();
            }

            for (int i = 0; i < unkGPSSize; i++)
            {
                unkGPS[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < unkDataSet6Count; i++)
            {
                unkSet6[i] = reader.ReadUInt16();
            }
        }

        public void WriteToFile()
        {
            DebugCheckLineIDX();
            UpdateData();
            using (BinaryWriter writer = new BinaryWriter(File.Open(info.FullName, FileMode.Create)))
            {
                WriteToFile(writer);
            }
        }
        public void WriteToFile(BinaryWriter writer)
        {
            writer.Write(magic);
            writer.Write(splineCount);
            writer.Write(splineCount);
            writer.WriteInt24(splineOffset);
            writer.Write(splinePropertiesCount);
            writer.Write(splinePropertiesCount);
            writer.WriteInt24(splinePropertiesOffset);
            writer.Write(junctionPropertiesCount);
            writer.Write(junctionPropertiesCount);
            writer.WriteInt24(junctionPropertiesOffset);
            writer.Write(unkDataSet3Count);
            writer.Write(unkDataSet3Count);
            writer.WriteInt24(unkDataSet3Offset);
            writer.Write(unkDataSet4Count);
            writer.Write(unkDataSet4Count);
            writer.WriteInt24(unkDataSet4Offset);
            writer.Write(unkGPSSize);
            writer.Write(unkGPSSize);
            writer.WriteInt24(unkDataSet5Offset);
            writer.Write(unkDataSet6Count);
            writer.Write(unkDataSet6Count);
            writer.Write((int)unkDataSet3Count);

            long[] positions = new long[splineCount];
            for (int i = 0; i < splineCount; i++)
            {
                SplineDefinition data = splines[i];
                positions[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.Offset);
                writer.Write(data.NumSplines1);
                writer.Write(data.NumSplines2);
                writer.Write(data.Length);
            }

            for (int i = 0; i < splineCount; i++)
            {
                long position = writer.BaseStream.Position;
                writer.BaseStream.Seek(positions[i], SeekOrigin.Begin);
                writer.WriteInt24((uint)position + 4);
                writer.BaseStream.Seek(position, SeekOrigin.Begin);
                SplineDefinition splineData = splines[i];
                for (int y = 0; y != splineData.Points.Length; y++)
                {
                    Vector3Utils.WriteToFile(splineData.Points[y], writer);
                }
            }

            WriteUpdatedOffset(writer, 8);
            positions = new long[splineData.Length]; //lane offset
            long[] pos2 = new long[splineData.Length]; //range offset

            for (int i = 0; i != splineData.Length; i++)
            {
                SplineProperties data = splineData[i];
                writer.Write(data.LaneIDX0);
                writer.Write(data.LaneIDX1);
                positions[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.Offset0);
                writer.Write(data.LaneSize0);
                writer.Write(data.LaneSize1);
                pos2[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.Offset1);
                writer.Write(data.RangeSize0);
                writer.Write(data.RangeSize1);
                writer.Write((short)data.Flags);
                writer.Write(data.SplineIDX);
            }

            long[] pos3;

            for (int i = 0; i < splineData.Length; i++)
            {
                SplineProperties data = splineData[i];
                //update lane position
                long curPosition = writer.BaseStream.Position;
                writer.BaseStream.Seek(positions[i], SeekOrigin.Begin);
                writer.WriteInt24((uint)curPosition + 4);
                writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                //finished
                //also do rangeoffset
                pos3 = new long[data.LaneSize0];

                for (int y = 0; y < data.LaneSize1; y++)
                {
                    LaneProperties sect = splineData[i].Lanes[y];
                    writer.Write(sect.Width);
                    writer.Write((ushort)sect.Flags);
                    writer.Write(sect.Unk03);
                    //writer.Write((ushort)0);
                    pos3[y] = writer.BaseStream.Position;
                    writer.WriteInt24(sect.RangeOffset);
                    writer.Write(sect.RangeSize0);
                    writer.Write(sect.RangeSize1);
                }

                for (int y = 0; y < data.LaneSize0; y++)
                {
                    ////update range for this lane
                    curPosition = writer.BaseStream.Position;
                    uint rangeOffset = (uint)(data.Lanes[y].RangeSize0 > 0 ? curPosition + 4 : 0);
                    writer.BaseStream.Seek(pos3[y], SeekOrigin.Begin);
                    writer.WriteInt24(rangeOffset);
                    writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                    //finished

                    for (int x = 0; x < data.Lanes[y].RangeSize0; x++)
                    {
                        RangeProperties sect = data.Lanes[y].Ranges[x];
                        writer.Write(sect.Unk01);
                        writer.Write(sect.Unk02);
                        writer.Write(sect.Unk03);
                        writer.Write(sect.Unk04);
                        writer.Write(sect.Unk05);
                    }
                }

                //update range position
                curPosition = writer.BaseStream.Position;
                uint laneOffset = (uint)(data.RangeSize0 > 0 ? curPosition + 4 : 0);
                writer.BaseStream.Seek(pos2[i], SeekOrigin.Begin);
                writer.WriteInt24(laneOffset);
                writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                //finished

                for (int y = 0; y < data.RangeSize0; y++)
                {
                    RangeProperties sect = data.Ranges[y];
                    writer.Write(sect.Unk01);
                    writer.Write(sect.Unk02);
                    writer.Write(sect.Unk03);
                    writer.Write(sect.Unk04);
                    writer.Write(sect.Unk05);
                }
            }

            WriteUpdatedOffset(writer, 16);

            positions = new long[junctionPropertiesCount];
            pos2 = new long[junctionPropertiesCount];
            pos3 = new long[junctionPropertiesCount];
            for (int i = 0; i < junctionPropertiesCount; i++)
            {
                JunctionDefinition data = junctionData[i];
                Vector3Utils.WriteToFile(data.Position, writer);
                positions[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.offset0);
                writer.Write(data.junctionSize0);
                writer.Write(data.junctionSize1);
                pos2[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.offset1);
                writer.Write(data.boundarySize0);
                writer.Write(data.boundarySize1);
                writer.Write(data.JunctionIDX);
                pos3[i] = writer.BaseStream.Position;
                writer.WriteInt24(data.offset2);
                writer.Write(data.Unk5);
                writer.Write(data.Unk6);
            }

            for (int i = 0; i < junctionPropertiesCount; i++)
            {
                //update junction position
                long curPosition = writer.BaseStream.Position;
                uint junctionOffset = (uint)(junctionData[i].junctionSize0 > 0 ? curPosition + 4 : 0);
                writer.BaseStream.Seek(positions[i], SeekOrigin.Begin);
                writer.WriteInt24(junctionOffset);
                writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                //finished

                long[] junctionPos = new long[junctionData[i].junctionSize0];
                for (int y = 0; y < junctionData[i].junctionSize0; y++)
                {
                    JunctionSpline data4Sect = junctionData[i].Splines[y];
                    writer.Write(data4Sect.Unk0);
                    writer.Write(data4Sect.Unk1);
                    writer.Write(data4Sect.Unk2);
                    writer.Write(data4Sect.Unk3);
                    writer.Write(data4Sect.Unk4);
                    writer.Write(data4Sect.Unk5);
                    junctionPos[y] = writer.BaseStream.Position;
                    writer.WriteInt24(data4Sect.Offset0);
                    writer.Write(data4Sect.PathSize0);
                    writer.Write(data4Sect.PathSize1);
                    writer.Write(data4Sect.Length);
                }
                for (int y = 0; y < junctionData[i].junctionSize0; y++)
                {
                    //update path vectors
                    curPosition = writer.BaseStream.Position;
                    uint pathOffset = (uint)(junctionData[i].Splines[y].PathSize0 > 0 ? curPosition + 4 : 0);
                    writer.BaseStream.Seek(junctionPos[y], SeekOrigin.Begin);
                    writer.WriteInt24(pathOffset);
                    writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                    //finished

                    for (int z = 0; z != junctionData[i].Splines[y].PathSize0; z++)
                    {
                        Vector3Utils.WriteToFile(junctionData[i].Splines[y].Path[z], writer);
                    }
                }

                //update boundary position
                curPosition = writer.BaseStream.Position;
                uint boundaryOffset = (uint)(junctionData[i].boundarySize0 > 0 ? curPosition + 4 : 0);
                writer.BaseStream.Seek(pos2[i], SeekOrigin.Begin);
                writer.WriteInt24(boundaryOffset);
                writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                //finished

                for (int y = 0; y < junctionData[i].boundarySize0; y++)
                {
                    Vector3Utils.WriteToFile(junctionData[i].Boundaries[y], writer);
                }

                //update unk position
                curPosition = writer.BaseStream.Position;
                uint unkOffset = (uint)(junctionData[i].Unk5 > 0 ? curPosition + 4 : 0);
                writer.BaseStream.Seek(pos3[i], SeekOrigin.Begin);
                writer.WriteInt24(unkOffset);
                writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                //finished

                if (junctionData[i].Unk5 >= 2)
                {
                    long offsetPos = 0;
                    long offset2Pos = 0;

                    unkStruct2Sect2 data = junctionData[i].DataSet2;

                    writer.Write(data.Unk0);
                    offsetPos = writer.BaseStream.Position;
                    writer.WriteInt24(data.Offset0);
                    writer.Write(data.Unk1);
                    writer.Write(data.Unk2);
                    writer.Write(data.Unk3);
                    offset2Pos = writer.BaseStream.Position;
                    writer.WriteInt24(data.Offset1);
                    writer.Write(data.Unk4);
                    writer.Write(data.Unk5);
                    writer.Write(data.Unk6);
                    writer.Write(data.Unk7);
                    writer.Write(data.Unk8);
                    writer.Write(data.Unk9);

                    if (junctionData[i].DataSet2.Offset1 - junctionData[i].DataSet2.Offset0 == 4)
                        Console.WriteLine("STOP");
                    else if (junctionData[i].DataSet2.Offset1 - junctionData[i].DataSet2.Offset0 == 8)
                        Console.WriteLine("STOP");
                    else
                        Console.WriteLine("STOP!");

                    //update offset0 position
                    curPosition = writer.BaseStream.Position;
                    writer.BaseStream.Seek(offsetPos, SeekOrigin.Begin);
                    writer.WriteInt24((uint)(curPosition - 4));
                    writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                    //finished
                    //update offset1 position
                    curPosition = writer.BaseStream.Position;
                    writer.BaseStream.Seek(offset2Pos, SeekOrigin.Begin);
                    writer.WriteInt24((uint)(data.Unk1 > 2 ? (curPosition-4) + (2 * (junctionData[i].DataSet2.Unk1)): curPosition));
                    writer.BaseStream.Seek(curPosition, SeekOrigin.Begin);
                    //finished
                    if (data.Unk1 > 2 && data.Unk2 > 2)
                    {
                        if (junctionData[i].DataSet2.Unk1 == 4)
                        {
                            writer.Write(junctionData[i].DataSet2.Unk10[0]);
                            writer.Write(junctionData[i].DataSet2.Unk10[1]);
                        }
                        else
                        {
                            writer.Write(junctionData[i].DataSet2.Unk10[0]);
                            writer.Write(junctionData[i].DataSet2.Unk10[1]);
                            writer.Write(junctionData[i].DataSet2.Unk10[2]);
                            writer.Write(junctionData[i].DataSet2.Unk10[3]);
                        }
                    }

                    if (junctionData[i].Unk5 == 3)
                    {
                        for(int x = 0; x < 8; x++)
                        {
                            writer.Write(junctionData[i].DataSet2.Unk3Bytes[x]);
                        }
                    }
                }
            }

            WriteUpdatedOffset(writer, 24);

            //related to junctions.
            for (int i = 0; i < unkDataSet3Count; i++)
                writer.Write(unkSet3[i]); //stops junctions working.

            WriteUpdatedOffset(writer, 32);

            for (int i = 0; i < unkDataSet4Count; i++)
            {
                writer.Write(unkSet4[i].Unk0);//stops cars spawning
                writer.Write(unkSet4[i].Unk1); //makes certain areas of the roadmap crash
            }

            WriteUpdatedOffset(writer, 40);

            for (int i = 0; i < unkGPSSize; i++)
                writer.Write(unkGPS[i]); //traffic works, but no GPS data is available.

            WriteUpdatedOffset(writer, 48);

            for (int i = 0; i < unkDataSet6Count; i++)
                writer.Write(unkSet6[i]); //causes crashes, junctions fail to work.
        }

        private void WriteUpdatedOffset(BinaryWriter writer, long pos, bool isZero = false)
        {
            long jump = writer.BaseStream.Position;
            writer.BaseStream.Seek(pos, SeekOrigin.Begin);
            writer.WriteInt24(isZero ? 0 : (uint)jump + 4);
            writer.BaseStream.Seek(jump, SeekOrigin.Begin);
        }

        public void DebugCheckLineIDX()
        {
            int correct = 0;
            List<ushort> unkset3 = new List<ushort>();

            ushort index = 0;
            for(int i = 0; i != splines.Length; i++)
            {
                SplineProperties data = splines[i].Toward;

                if (data != null)
                {
                    int length = data.Lanes.Length;
                    if(data.Flags.HasFlag(RoadFlags.flag_2048))
                    {
                        length /= 2;
                    }

                    for(int z = 0; z < length; z++)
                    {
                        if (data.Lanes[z].Flags != 0)
                        {
                            unkset3.Add(index);
                        }
                        index++;
                    }   
                }
            }
            Console.WriteLine(correct + "/" + splineData.Length + " = " + Convert.ToSingle(correct / splineData.Length));
        }

        public void UpdateData()
        {
            List<SplineProperties> splineList = new List<SplineProperties>();

            for (int i = 0; i < splines.Length; i++)
            {
                //update spline length
                SplineDefinition data = splines[i];
                float curDistance = 0.0f;

                for (int x = 0; x < data.NumSplines1; x++)
                {
                    float temp = 0.0f;

                    if (x < data.NumSplines1 - 1)
                    {
                        temp += Vector3.Distance(data.Points[x], data.Points[x + 1]);
                    }

                    if (x > 0)
                    {
                        temp += Vector3.Distance(data.Points[x], data.Points[x - 1]);
                    }

                    curDistance += temp;
                }
                data.Length = curDistance/2;

                //update road meta data.
                if (data.HasToward)
                {
                    splineList.Add(data.Toward);
                    data.Toward.SplineIDX = (ushort)(i + data.IndexOffset);
                    data.Toward.LaneSize0 = data.Toward.LaneSize1 = (ushort)data.Toward.Lanes.Length;

                    if (data.Toward.Ranges != null)
                    {
                        data.Toward.RangeSize0 = data.Toward.RangeSize1 = (ushort)data.Toward.Ranges.Length;
                    }
                }

                if (data.HasBackward)
                {
                    splineList.Add(data.Backward);
                    data.Backward.SplineIDX = (ushort)(i + data.IndexOffset);
                    data.Backward.LaneSize0 = data.Backward.LaneSize1 = (ushort)data.Backward.Lanes.Length;

                    if (data.Backward.Ranges != null)
                    {
                        data.Backward.RangeSize0 = data.Backward.RangeSize1 = (ushort)data.Backward.Ranges.Length;
                    }
                }

                splines[i] = data;
            }

            for(int i = 0; i < junctionData.Length; i++)
            {
                JunctionDefinition data = junctionData[i];
                data.boundarySize0 = data.boundarySize1 = (short)(data.Boundaries != null ? data.Boundaries.Length : 0);
                data.junctionSize0 = data.junctionSize1 = (short)(data.Splines != null ? data.Splines.Length : 0);

                if (data.Splines != null)
                {
                    for (int x = 0; x < data.Splines.Length; x++)
                    {
                        data.Splines[x].PathSize0 = data.Splines[x].PathSize1 = (short)data.Splines[x].Path.Length;

                        float curDistance = 0.0f;
                        for (int z = 0; z < data.Splines[x].PathSize0; z++)
                        {
                            float temp = 0.0f;

                            if (z < data.Splines[x].PathSize0 - 1)
                            {
                                temp += Vector3.Distance(data.Splines[x].Path[z], data.Splines[x].Path[z + 1]);
                            }

                            if (z > 0)
                            {
                                temp += Vector3.Distance(data.Splines[x].Path[z], data.Splines[x].Path[z - 1]);
                            }

                            curDistance += temp;
                        }
                        data.Splines[x].Length = curDistance / 2;
                    }
                }
            }

            //List<ushort> list = unkSet3.ToList();
            //List<ushort> list2 = unkSet5.ToList();

            //int size = splineList.Count*2;
            //for(int i = 0; list.Count != size; i++)
            //{
            //    list.Add(0);
            //}

            //int lateIDX = unkSet5[unkSet5.Length - 1];
            //for (int i = list2.Count; i != (list.Count+2/2); i++)
            //{
            //    list2.Add((ushort)++lateIDX);
            //}

            //unkSet3 = list.ToArray();
            //unkSet5 = list2.ToArray();
            unkDataSet3Count = (ushort)unkSet3.Length;
            unkDataSet4Count = (ushort)unkSet4.Length;
            unkGPSSize = (ushort)unkGPS.Length;
            unkDataSet6Count = (ushort)unkSet6.Length;
            splineCount = (ushort)splines.Length;
            splineData = splineList.ToArray();
            splinePropertiesCount = (ushort)splineData.Length;
            junctionPropertiesCount = (ushort)junctionData.Length;
        }
    }
}
