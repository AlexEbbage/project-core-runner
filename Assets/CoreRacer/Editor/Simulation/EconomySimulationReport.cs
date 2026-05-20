using System.Text;

namespace CoreRacer.Editor.Simulation
{
    public sealed class EconomySimulationReport
    {
        public int Runs;
        public int SoftCurrencyEarned;
        public int PremiumCurrencyEarned;
        public int EstimatedRewardedAdsWatched;
        public int RunsToFirstUpgrade;
        public int RunsToSecondShip;

        public string ToMarkdown()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Core Racer Economy Simulation Report");
            sb.AppendLine();
            sb.AppendLine($"- Runs simulated: {Runs}");
            sb.AppendLine($"- Soft currency earned: {SoftCurrencyEarned}");
            sb.AppendLine($"- Premium currency earned: {PremiumCurrencyEarned}");
            sb.AppendLine($"- Rewarded ads watched estimate: {EstimatedRewardedAdsWatched}");
            sb.AppendLine($"- Runs to first upgrade: {RunsToFirstUpgrade}");
            sb.AppendLine($"- Runs to second ship: {RunsToSecondShip}");
            sb.AppendLine();
            sb.AppendLine("## Use this report to tune");
            sb.AppendLine("- First upgrade should usually be reachable in the first 2–5 runs.");
            sb.AppendLine("- Second ship should feel aspirational without blocking early fun.");
            sb.AppendLine("- Rewarded ads should accelerate progression without becoming mandatory.");
            return sb.ToString();
        }
    }
}
