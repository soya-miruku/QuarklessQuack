﻿using ContentSearcher;
using InstagramApiSharp;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.RestSharpClient;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessLogic.Handlers.ClientProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Services.Extensions;
using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.ContentBuilder.ContentSearch
{
	public class ContentSearch : IContentSearch
	{
		private readonly IRestSharpClientManager _restSharpClient;
		private readonly IAPIClientContext _context;
		private readonly YandexImageSearch _yandexImageSearch;
		public ContentSearch(IRestSharpClientManager restSharpClient, IAPIClientContext clientContext)
		{
			_restSharpClient = restSharpClient;
			_context = clientContext;
			_yandexImageSearch = new YandexImageSearch();
		}
		
		public async Task<Media> SearchMediaInstagram(UserStore user, List<string> topics, InstaMediaType mediaType, int limit)
		{
			IAPIClientContainer _container = new APIClientContainer(_context,user.AccountId,user.InstaAccountId);
			Media mediaresp = new Media();
			foreach(var topic in topics) { 
				MediaResponse media_ = new MediaResponse();
				var results = await _container.Hashtag.GetTopHashtagMediaListAsync(topic,PaginationParameters.MaxPagesToLoad(limit));
				if (results.Succeeded)
				{
					media_.Topic = topic;
					foreach(var results_media in results.Value.Medias.Where(a=>a.MediaType == mediaType))
					{
						switch (mediaType)
						{
							case InstaMediaType.Image:
								media_.MediaUrl.Add(results_media.Images.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Video:
								media_.MediaUrl.Add(results_media.Videos.FirstOrDefault().Uri);
								break;
							case InstaMediaType.Carousel:
								List<string> total_ = new List<string>();
								results_media.Carousel.Select(s=> 
								{
									media_.MediaUrl.Add(s.Videos.FirstOrDefault().Uri);
									media_.MediaUrl.Add(s.Images.FirstOrDefault().Uri);
									return s;
								});
								if(total_.Count>0)
									media_.MediaUrl.AddRange(total_);
								break;
						}
					}
					mediaresp.Medias.Add(media_);		
				}
			}
			return mediaresp;
		}

		public async Task<Media> SearchMediaUser(UserStore user, int limit = 1)
		{
			IAPIClientContainer _container = new APIClientContainer(_context, user.AccountId, user.InstaAccountId);
			Media mediaresp = new Media();
			var results = await _container.User.GetUserMediaAsync(_container.GetContext.InstagramAccount.Username, PaginationParameters.MaxPagesToLoad(limit));
			if (results.Succeeded)
			{
				MediaResponse media = new MediaResponse();
				foreach(var lema in results.Value)
				{
					switch (lema.MediaType)
					{
						case InstaMediaType.Image:
							media.MediaUrl.Add(lema.Images.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Video:
							media.MediaUrl.Add(lema.Videos.FirstOrDefault().Uri);
							break;
						case InstaMediaType.Carousel:
							lema.Carousel.Select(s =>
							{
								media.MediaUrl.Add(s.Videos.FirstOrDefault().Uri);
								media.MediaUrl.Add(s.Images.FirstOrDefault().Uri);
								return s;
							});
							break;
					}		
				}
				mediaresp.Medias.Add(media);
			}
			return mediaresp;
		}

		/// <summary>
		/// search images via google
		/// todo, add private url
		/// </summary>
		/// <param name="searchImageQuery"></param>
		/// <returns></returns>
		public Media SearchViaGoogle(SearchImageModel searchImageQuery)
		{
			var results = _restSharpClient.PostRequest("http://127.0.0.1:5000/","searchImages",JsonConvert.SerializeObject(searchImageQuery),null);
			if (results.IsSuccessful)
			{
				Media responseValues = JsonConvert.DeserializeObject<Media>(results.Content);
				return responseValues;
			}
			return null;
		}
		public Media SearchViaYandexBySimilarImages(List<GroupImagesAlike> imagesSimilarUrls, int limit)
		{
			var images =  _yandexImageSearch.Search(imagesSimilarUrls, limit);
			return images;
		}
	}
}
