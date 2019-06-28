﻿using Quarkless.Queue.Interfaces.Jobs;
using QuarklessContexts.Models.Timeline;
using System;

namespace Quarkless.Queue.Jobs.JobOptions
{
	public class LongRunningJobOptions : IJobOptions
	{
		public string ActionName { get; set; }
		public RestModel Rest { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}
