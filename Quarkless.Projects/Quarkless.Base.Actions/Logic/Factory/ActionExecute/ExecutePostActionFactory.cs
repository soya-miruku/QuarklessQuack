﻿using Quarkless.Analyser;
using Quarkless.Base.Actions.Logic.Action_Executes;
using Quarkless.Base.Actions.Models.Factory;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.Actions.Logic.Factory.ActionExecute
{
	public class ExecutePostActionFactory: ActionExecuteFactory
	{
		private readonly IPostAnalyser _postAnalyser;
		public ExecutePostActionFactory(IPostAnalyser postAnalyser)
		{
			_postAnalyser = postAnalyser;
		}
		public override IActionExecute Create(IWorker worker, IResponseResolver resolver)
			=> new CreatePostExecute(worker, resolver, _postAnalyser);
	}
}