using UnityEngine;

namespace Tests.Experiments
{
    public class NaiveAplibExperiment : BaseAplibExperiment
    {
        protected override AgentType AgentType => AgentType.Aplib;
        protected override bool IsJumping => isJumping;
        private bool isJumping = false;

        protected override bool hasReachedJumpTarget(ExperimentBeliefSet beliefSet)
        {
            return isJumping && checkNextFiveNodes(beliefSet);
        }

        protected override void StartJump(System.Tuple<int, Vector2> jumpEndPoint)
        {
            inputGenerator.HoldJump();
            isJumping = true;
        }

        protected override void StopJump()
        {
            inputGenerator.ReleaseJump();
            isJumping = false;
        }
    }
}
