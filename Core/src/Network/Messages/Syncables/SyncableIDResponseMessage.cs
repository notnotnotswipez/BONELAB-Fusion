﻿using LabFusion.Data;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public class SyncableIDResponseData : IFusionSerializable, IDisposable
    {
        public ushort queuedId;
        public ushort newId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(queuedId);
            writer.Write(newId);
        }

        public void Deserialize(FusionReader reader) {
            queuedId = reader.ReadUInt16();
            newId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SyncableIDResponseData Create(ushort queuedId, ushort newId) {
            return new SyncableIDResponseData() {
                queuedId = queuedId,
                newId = newId
            };
        }
    }

    public class SyncableIDResponseMessage : FusionMessageHandler {
        public override byte? Tag => NativeMessageTag.SyncableIDResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // Make sure this isn't a server
            if (!NetworkInfo.IsServer && !isServerHandled) {
                using (var reader = FusionReader.Create(bytes))
                {
                    using (var data = reader.ReadFusionSerializable<SyncableIDResponseData>())
                    {
                        SyncUtilities.UnqueueSyncable(data.queuedId, data.newId, out var syncable);

#if DEBUG
                        FusionLogger.Log($"Unqueued syncable with new id {syncable.GetId()}");
#endif
                    }
                }
            }
        }
    }
}