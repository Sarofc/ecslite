namespace Leopotam.EcsLite.Extension
{
    public static class EcsEventExt
    {
        public static void SendMessage<T>(this EcsWorld self, in T e) where T : struct
        {
            var ent = self.NewEntity();

            {
                ref var msg = ref ent.Add<T>(self);
                msg = e;
            }

            //Log.ERROR($"SendMessage: {@event}");
        }

        public static void SendMessage<T1, T2>(this EcsWorld self, in T1 e1, in T2 e2) where T1 : struct where T2 : struct
        {
            var ent = self.NewEntity();

            {
                ref var msg = ref ent.Add<T1>(self);
                msg = e1;
            }

            {
                ref var msg = ref ent.Add<T2>(self);
                msg = e2;
            }

            //Log.ERROR($"SendMessage: {evt1} {evt2}");
        }
    }
}
