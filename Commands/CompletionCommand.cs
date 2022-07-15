using Terraria.ModLoader;
using Terraria.Achievements;
using System;
using TerrariaArchipelago.Common;

namespace Archipelago.Commands
{
	public class CompletionCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "completion";

		public override string Description
			=> "/completion amount";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if(args.Length == 0)
            {
				args = new string[] { "5" };
            }
			int amount;
			bool isNumber = Int32.TryParse(args[0], out amount);
			if(!isNumber)
            {
				TextUtils.SendText("Please insert a number for the amount.");
            }
			TextUtils.SendText("Achievements Not Yet Completed: ");
			foreach(Achievement achievement in Achievements.AchievementManager.achievements.Keys)
            {
				if(amount <= 0)
                {
					return;
                }
				if (Achievements.AchievementManager.achievements_completed.Contains(achievement))
				{
					continue;
				}
				TextUtils.SendText(achievement.FriendlyName + " Is Not Completed.");
				amount--;
            }
		}
	}
}