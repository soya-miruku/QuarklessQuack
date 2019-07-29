﻿using Newtonsoft.Json;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessRepositories.RedisRepository.RedisClient;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.AccountCache
{
	public class AccountCache : IAccountCache
	{
		private readonly IRedisClient _redisClient;
		public AccountCache(IRedisClient redisClient)
		{
			_redisClient = redisClient;
		}
		public async Task<AccountUser> GetAccount(string userId)
		{
			string userkeyid = $"{userId}";
			var res = await _redisClient.Database(0).StringGet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions);
			if (res != null && !string.IsNullOrEmpty(res))
			{
				return JsonConvert.DeserializeObject<AccountUser>(res);
			}
			return null;
		}
		public async Task SetAccount(AccountUser value)
		{
			string userkeyid = $"{value.UserName}";

			await _redisClient.Database(0).StringSet(userkeyid, RedisKeys.HashtagGrowKeys.Usersessions, JsonConvert.SerializeObject(value));
		}
	}
}
