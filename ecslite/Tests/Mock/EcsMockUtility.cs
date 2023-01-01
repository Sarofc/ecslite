using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saro.Entities.Tests
{
    internal class EcsMockUtility
    {
        public static EcsSystems CreateMockSystem()
        {
            var world = new EcsWorld("test");
            var systems = new EcsSystems(world);
            //systems.Add(new TestSystemUnmanaged());
            return systems;
        }
    }
}
