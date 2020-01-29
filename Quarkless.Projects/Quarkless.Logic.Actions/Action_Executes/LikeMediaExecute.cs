﻿using System;
using System.Threading.Tasks;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Action_Executes
{
	internal class LikeMediaExecute : IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;
		internal LikeMediaExecute(IWorker worker, IResponseResolver responseResolver)
		{
			_worker = worker;
			_responseResolver = responseResolver;
		}

		public async Task<ResultCarrier<bool>> ExecuteAsync(EventExecuteBody eventAction)
		{
			var result = new ResultCarrier<bool>();
			try
			{
				Console.WriteLine($"Started to execute {nameof(GetType)} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");
				
				if (!(eventAction.Body is string mediaId))
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse { Message = "Media Request is empty" };
					return result;
				}

				var response = await _responseResolver.WithClient(_worker.Client)
					.WithResolverAsync(await _worker.Client.Media.LikeMediaAsync(mediaId),
						ActionType.LikePost, mediaId);

				if (!response.Succeeded)
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse
					{
						Message = response.Info.Message,
						Exception = response.Info.Exception
					};
					return result;
				}

				result.IsSuccessful = true;
				result.Results = response.Succeeded;
				return result;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Message = err.Message,
					Exception = err
				};
				return result;
			}
			finally
			{
				Console.WriteLine($"Ended execute {nameof(GetType)} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");
			}
		}
	}
}