﻿using System;
using System.IO;
using EHVAG.DemoInfo.Utils;

namespace EHVAG.DemoInfo.ProtobufMessages
{
    public struct CreateStringTable
    {
        public string Name;
        public Int32 MaxEntries;
        public Int32 NumEntries;
        private Int32 _UserDataFixedSize;
        public bool UserDataFixedSize { get { return _UserDataFixedSize != 0; } }
        public Int32 UserDataSize;
        public Int32 UserDataSizeBits;
        public Int32 Flags;

        internal void Parse(IBitStream bitstream, DemoParser parser)
        {
            while (!bitstream.ChunkFinished)
            {
                var desc = bitstream.ReadProtobufVarInt();
                var wireType = desc & 7;
                var fieldnum = desc >> 3;

                if (wireType == 2)
                {
                    if (fieldnum == 1)
                    {
                        Name = bitstream.ReadProtobufString();
                        continue;
                    }
                    else if (fieldnum == 8)
                    {
                        // String data is special.
                        // We'll simply hope that gaben is nice and sends
                        // string_data last, just like he should.
                        var len = bitstream.ReadProtobufVarInt();
                        bitstream.BeginChunk(len * 8);

                        StringTables.StringTable.ParseCreateStringTable(parser, bitstream, this);

                        bitstream.EndChunk();
                        if (!bitstream.ChunkFinished)
                            throw new NotImplementedException("PacketEntities packet was in an order we can't handle (although it's valid!). Please open an issue on GitHub");
                        break;
                    }
                }

                if (wireType != 0)
                    throw new InvalidDataException();

                var val = bitstream.ReadProtobufVarInt();

                switch (fieldnum)
                {
                    case 2:
                        MaxEntries = val;
                        break;
                    case 3:
                        NumEntries = val;
                        break;
                    case 4:
                        _UserDataFixedSize = val;
                        break;
                    case 5:
                        UserDataSize = val;
                        break;
                    case 6:
                        UserDataSizeBits = val;
                        break;
                    case 7:
                        Flags = val;
                        break;
                    default:
                        // silently drop
                        break;
                }
            }
        }
    }
}

