using UnityEngine;

namespace Tests.Experiments
{
    public class HybridExperiment : BaseAplibExperiment
    {
        protected override AgentType AgentType => AgentType.Hybrid;
        protected override bool IsJumping => jumpTacticAgent.enabled;

        protected override bool hasReachedJumpTarget(ExperimentBeliefSet beliefSet)
        {
            return jumpTacticAgent.enabled && (jumpTacticAgent.ReachedGoal || checkNextFiveNodes(beliefSet));
        }

        protected override void StartJump(System.Tuple<int, Vector2> jumpEndPoint)
        {
            jumpTacticAgent.StartAgent(jumpEndPoint);
        }

        protected override void StopJump()
        {
            jumpTacticAgent.StopAgent();
        }
    }
}
