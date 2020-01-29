﻿using System;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class LikeActionOptions : IActionOptions
	{
		public LikeActionType LikeActionType { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(45, 75);
		public LikeStrategySettings StrategySettings { get; set; } = new LikeStrategySettings();
		public LikeActionOptions(LikeActionType likeActionType = LikeActionType.Any, 
			LikeStrategySettings strategySettings = null, XRange? timeFrame = null)
		{
			this.LikeActionType = likeActionType;

			if (strategySettings != null)
				StrategySettings = strategySettings;

			if (timeFrame.HasValue)
				TimeFrameSeconds = timeFrame.Value;
		}
	}
}