﻿using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public interface IWorkerManager
	{
		event EventHandler<WorkerManagerUpdateEventArgs> WorkerManagerUpdateStarting;
		event EventHandler<WorkerManagerUpdateEventArgs> WorkerManagerFinishedUpdating;
		Task PerformTaskOnWorkers(Func<IWorkers, Task> action);
		Task PerformTaskOnWorkers(Action<IWorkers> action);
		Task PerformTaskOnWorkers(Func<IWorkers, string, Task> action, string topic);
	}
}