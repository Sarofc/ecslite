//namespace Saro.Entities
//{
//    public partial class EcsWorld
//    {
//        /// <summary>
//        /// TODO 设计上，是为了砍掉<see cref="EcsPackedEntityWithWorld"/>，简化api调用。每个<see cref="EcsSystems"/>在tick时，设置自己持有的world
//        /// 好像行不通。。。 外部持有的entity无法使用这个来约束
//        /// </summary>
//        public static EcsWorld Current { get; set; }
//    }
//}