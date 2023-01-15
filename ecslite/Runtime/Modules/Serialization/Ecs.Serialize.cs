using System;
using Saro.Entities.Serialization;

namespace Saro.Entities
{
	partial class Name : IEcsSerializable
	{
		public void Deserialize(IEcsReader reader)
		{
			name = reader.ReadString();
		}

		public void Serialize(IEcsWriter writer)
		{
			writer.Write(name);
		}
	}
}
