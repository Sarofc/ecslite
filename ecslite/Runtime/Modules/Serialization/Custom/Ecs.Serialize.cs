using System;
using Saro.Entities.Serialization;
using Saro.FSnapshot;

namespace Saro.Entities
{
    [FSnapshotable]
    partial class Name : IFSnapshotable
    {
        public void RestoreSnapshot(ref FSnapshotReader reader)
        {
            name = reader.ReadString();
        }

        public void TakeSnapshot(ref FSnapshotWriter writer)
        {
            writer.WriteString(name);
        }
    }
}
