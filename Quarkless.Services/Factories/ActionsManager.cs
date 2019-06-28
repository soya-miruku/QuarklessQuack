﻿using Quarkless.Services.Factories.Actions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;

namespace Quarkless.Services.Factories
{
	public class ActionsManager
	{
		private readonly Dictionary<ActionType, ActionBuilderFactory> _factories;

		public ActionsManager()
		{
			_factories = new Dictionary<ActionType, ActionBuilderFactory>
			{
				{ ActionType.FollowUser, new FollowUserActionBuilderFactory() },
				{ ActionType.CreatePostTypeImage, new CreateImagePostActionBuilderFactory() },
				{ ActionType.CreatePostTypeVideo, new CreateVideoPostActionBuilderFactory() },
				{ ActionType.LikePost, new LikeMediaPostActionBuilderFactory()},
				{ ActionType.CreateCommentMedia, new CreateCommentMediaActionBuilderFactory() },
			};
		}

		public static ActionsManager Begin => new ActionsManager();

		public IActionCommit Commit(ActionType actionType,IContentManager actionBuilderManager,
			ProfileModel profile, UserStore user) =>_factories[actionType].Commit(actionBuilderManager,profile,user);
	}
}
