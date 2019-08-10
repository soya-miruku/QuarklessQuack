﻿using Newtonsoft.Json;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;
using QuarklessRepositories.RedisRepository.RedisClient;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessRepositories.RedisRepository.SearchCache
{
	public class SearchingCache : ISearchingCache
	{
		private readonly IRedisClient _redis;
		public SearchingCache(IRedisClient redisClient)
		{
			_redis = redisClient;
		}
		public async Task StoreSearchData(string userId, SearchRequest search, string instagramId = null, string profileId = null)
		{
			try
			{
				RedisKey key = $"{userId}";
				if (!string.IsNullOrEmpty(instagramId))
					key = key.Append($":{instagramId}");

				if (!string.IsNullOrEmpty(profileId))
					key = key.Append($":{profileId}");

				key = key.Append($":{search.Offset.ToString()}");

				await _redis.SetAdd(key, RedisKeys.HashtagGrowKeys.SearchSession, JsonConvert.SerializeObject(search), TimeSpan.FromHours(1));
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
			}
		}
		public async Task<SearchRequest> GetSearchData(string userId, SearchRequest find, string instagramId = null, string profileId = null)
		{
			try
			{
				RedisKey key = $"{userId}";
				if (!string.IsNullOrEmpty(instagramId))
					key = key.Append($":{instagramId}");

				if (!string.IsNullOrEmpty(profileId))
					key = key.Append($":{profileId}");

				key = key.Append($":{find.Offset.ToString()}");

				var res = await _redis.GetMembers<SearchRequest>(key, RedisKeys.HashtagGrowKeys.SearchSession);
				if(res==null) return null;
				var req = res.Select(s=>s.RequestData).ToList();
				var findMe = req.FindIndex(s=>s.SequenceEqual(find.RequestData));
				if (findMe >= 0)
				{
					return res.ElementAtOrDefault(findMe);
				}
				return null;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public async Task<SubTopics> GetReleatedTopic(string topic)
		{
			RedisKey key = $"ReleatedTopics:{topic}";
			try
			{
				var res = await _redis.GetMembers<SubTopics>(key, RedisKeys.HashtagGrowKeys.SearchSession);
				return res.FirstOrDefault();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return null;
			}
		}
		public async Task StoreRelatedTopics(SubTopics subTopics)
		{
			try
			{
				RedisKey key = $"ReleatedTopics:{subTopics.TopicName}";
				await _redis.SetAdd(key,RedisKeys.HashtagGrowKeys.SearchSession, JsonConvert.SerializeObject(subTopics),TimeSpan.FromDays(9999));
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
