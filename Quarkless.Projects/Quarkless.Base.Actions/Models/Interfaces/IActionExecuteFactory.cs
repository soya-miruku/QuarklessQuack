﻿using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Actions.Models.Interfaces
{
	/// <summary>
	/// Requires IInstagramAccountLogic, IApiClientContext & IResponseResolver Interface Injected
	/// </summary>
	public interface IActionExecuteFactory
	{
		public IActionExecute Create(ActionType action, in UserStore userStore);
	}
}