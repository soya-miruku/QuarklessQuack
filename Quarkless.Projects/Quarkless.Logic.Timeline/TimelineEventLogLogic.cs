﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Newtonsoft.Json.Linq;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Quarkless.Models.Timeline.Interfaces;

namespace Quarkless.Logic.Timeline
{
	public class TimelineEventLogLogic : ITimelineEventLogLogic
	{
		private readonly ITimelineLoggingRepository _timelineLoggingRepository;
		private readonly ITimelineLoggingRepositoryMongo _timelineLoggingRepositoryMongo;

		public TimelineEventLogLogic(ITimelineLoggingRepository timelineLoggingRepository, ITimelineLoggingRepositoryMongo timelineLoggingRepositoryMongo)
		{
			_timelineLoggingRepository = timelineLoggingRepository;
			_timelineLoggingRepositoryMongo = timelineLoggingRepositoryMongo;
		}

		public async Task AddTimelineLogFor(TimelineEventLog timelineEvent)
		{
			await _timelineLoggingRepositoryMongo.AddTimelineLogFor(timelineEvent);
		}

		public async Task<IEnumerable<TimelineEventLog>> GetLogsForUser(string accountId, string instagramAccountId,
			int limit, int level = 1, TimelineEventStatus? status = null)
		{
			return await _timelineLoggingRepositoryMongo.GetLogsForUser(accountId, instagramAccountId, limit, level, status); ;
		}

		public async Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types)
		{
			var occurrences = 0;
			var logs = await GetLogsForUser(accountId, instagramAccountId, limit);

			foreach (var timelineEventLog in logs.OrderByDescending(s=>s.DateAdded))
			{
				try
				{
					var respType = JObject.Parse(timelineEventLog.Response)["Info"]["ResponseType"].Value<int>();
					if (types.Contains((ResponseType) respType))
					{
						occurrences++;
					}
				}
				catch
				{
					continue;
				}
			}

			return occurrences;
		}
		public async Task<IEnumerable<TimelineEventLog>> GetLogsByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params ResponseType[] types)
		{
			var items = new List<TimelineEventLog>();
			var logs = await GetLogsForUser(accountId, instagramAccountId, limit);

			foreach (var timelineEventLog in logs.OrderByDescending(s => s.DateAdded))
			{
				try
				{
					var respType = JObject.Parse(timelineEventLog.Response)["Info"]["ResponseType"].Value<int>();
					if (types.Contains((ResponseType)respType))
					{
						items.Add(timelineEventLog);
					}
				}
				catch
				{
					continue;
				}
			}

			return items;
		}
	}
}
