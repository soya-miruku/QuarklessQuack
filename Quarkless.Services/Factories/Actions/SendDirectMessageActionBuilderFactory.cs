﻿using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Interfaces;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.Factories.Actions
{
	public class SendDirectMessageActionBuilderFactory : ActionBuilderFactory
	{
		public override IActionCommit Commit(IContentManager builder, IHeartbeatLogic heartbeatLogic)
			=> new DirectMessagingAction(builder,heartbeatLogic);
	}
}
