﻿using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.QueryModels;
using QuarklessContexts.Models.QueryModels.Settings;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.ContentSearch;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.ServicesLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessRepositories.RedisRepository.SearchCache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.QueryLogic
{
	public class QueryLogic : IQueryLogic
	{
		private readonly IRestSharpClientManager _restSharpClientManager;
		private readonly IContentSearcherHandler _contentSearcherHandler;
		private readonly ITopicServicesLogic _topicServicesLogic;
		private readonly ISearchingCache _searchingCache;
		private readonly IHashtagLogic _hashtagLogic;

		public QueryLogic(IRestSharpClientManager restSharpClientManager, IContentSearcherHandler contentSearcherHandler,
			ISearchingCache searchingCache, ITopicServicesLogic topicServicesLogic, IHashtagLogic hashtagLogic)
		{
			_hashtagLogic = hashtagLogic;
			_searchingCache = searchingCache;
			_restSharpClientManager = restSharpClientManager;
			_contentSearcherHandler = contentSearcherHandler;
			_topicServicesLogic = topicServicesLogic;
		}
		public object SearchPlaces(string query)
		{
			try
			{
				var results = _restSharpClientManager.GetRequest("https://maps.googleapis.com/maps/api/place/textsearch/json?query=" + query + "&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null);

				return results.Content;
			}
			catch(Exception ee)
			{
				return null;
			}
		}
		public object AutoCompleteSearchPlaces(string query, int radius = 500)
		{
			try
			{
				return _restSharpClientManager.GetRequest($"https://maps.googleapis.com/maps/api/place/autocomplete/json?input={query}&radius={radius}&key=AIzaSyD9hK0Uc_QZ-ejA6cXKrEdCJAOmerEsp0s",null).Content;
			}
			catch(Exception e)
			{
				return null;
			}
		}
		public async Task<Media> SimilarImagesSearch(string userId, int limit = 1, int offset = 0, IEnumerable<string> urls = null)
		{
			if(urls == null || limit < 1) return null;
			var strurl = urls.Select(_ => new QuarklessContexts.Models.Profiles.GroupImagesAlike { TopicGroup = "any", Url = _ }).ToList();
			int newLimit = limit;
			if(offset==1) offset = 0;
			if(offset > 0)
			{
				newLimit*=(offset);
			}

			SearchRequest searchRequest = new SearchRequest
			{
				RequestData = urls,
				Limit = newLimit,
				Offset = offset > 0 ? Math.Abs(newLimit - limit) : offset,
			};
			Media response = null;
			var cacheRes = await _searchingCache.GetSearchData(userId, searchRequest);
			if (cacheRes!=null)
			{
				response = cacheRes.ResponseData;
				return response;
			}
			var res = _contentSearcherHandler.SearchSimilarImagesViaGoogle(strurl, newLimit, offset > 0 ? Math.Abs(newLimit-limit) : offset);
			if(res.StatusCode == QuarklessContexts.Models.ResponseModels.ResponseCode.Success)
			{
				response = res.Result;
				searchRequest.ResponseData = response;
				await _searchingCache.StoreSearchData(userId,searchRequest);
			}
			else
			{
				response = _contentSearcherHandler.SearchViaYandexBySimilarImages(strurl,1+offset,offset).Result;
				response.Medias.Reverse();
				//response.Medias = response.Medias.Take(limit).ToList();
				searchRequest.ResponseData = response;
				await _searchingCache.StoreSearchData(userId, searchRequest);
			}


			return response;
		}
		public async Task<ProfileConfiguration> GetProfileConfig()
		{
			return new ProfileConfiguration
			{
				Topics = await _topicServicesLogic.GetAllTopicCategories(),
				ColorsAllowed = Enum.GetValues(typeof(ColorType)).Cast<ColorType>().Select(v=>v.GetDescription()),
				ImageTypes = Enum.GetValues(typeof(ImageType)).Cast<ImageType>().Select(v=>v.GetDescription()),
				Orientations = Enum.GetValues(typeof(Orientation)).Cast<Orientation>().Select(v=>v.GetDescription()),
				SizeTypes = Enum.GetValues(typeof(SizeType)).Cast<SizeType>().Select(v=>v.GetDescription()),
				SearchTypes = Enum.GetValues(typeof(SearchType)).Cast<SearchType>().Select(v=>v.GetDescription()),
				Languages = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(_=>!_.Name.Contains("-")).Distinct().ToDictionary(_ => _.Name, _ => _.EnglishName),
				CanUserEditProfile = true
			};
		}
		public async Task<SubTopics> GetReleatedKeywords(string topicName)
		{
			var res = await _searchingCache.GetReleatedTopic(topicName);
			if (res == null)
			{
				var hashtagsRes = await _hashtagLogic.SearchHashtagAsync(topicName);
				if (!hashtagsRes.Succeeded)
				{
					var releated = await _hashtagLogic.SearchReleatedHashtagAsync(topicName, 1);
					if (releated.Succeeded)
					{
						SubTopics subTopics = new SubTopics
						{
							TopicName = topicName,
							RelatedTopics = releated.Value.RelatedHashtags.Select(s => s.Name).ToList()
						};
						await _searchingCache.StoreRelatedTopics(subTopics);
						return subTopics;
					}
					else
					{
						return null;
					}
				}
				else
				{
					SubTopics subTopics = new SubTopics
					{
						TopicName = topicName,
						RelatedTopics = hashtagsRes.Value.Select(s=>s.Name).ToList()
					};
					await _searchingCache.StoreRelatedTopics(subTopics);
					return subTopics;
				}
			}
			else
			{
				return res;
			}
		}
	}
}
