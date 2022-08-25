using System.Collections.Generic;

namespace ET
{
    [ChildOf(typeof(RobotCaseComponent))]
    public class RobotCase: Entity, IAwake
    {
        public ETCancellationToken CancellationToken;
        public string CommandLine;
    }
}