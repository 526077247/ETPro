﻿namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class ActorLocationSenderComponent: Entity, IAwake, IDestroy
    {
        public static long TIMEOUT_TIME = 60 * 1000;

        public static ActorLocationSenderComponent Instance { get; set; }

        public long CheckTimer;
    }
}