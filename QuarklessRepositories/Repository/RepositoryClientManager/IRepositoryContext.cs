﻿using Hangfire.Mongo.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Library;
using QuarklessContexts.Models.Logger;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Proxies;
using QuarklessContexts.Models.ServicesModels;
using QuarklessContexts.Models.ServicesModels.Corpus;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessContexts.Models.Topics;

namespace QuarklessRepositories.RepositoryClientManager
{
	public interface IRepositoryContext
	{
		IMongoCollection<InstagramAccountModel> InstagramAccounts { get; }
		IMongoCollection<ProxyModel> Proxies { get;}
		IMongoCollection<AccountUser> Account { get; }
		IMongoCollection<ProfileModel> Profiles { get; }
		IMongoCollection<LoggerModel> Logger { get; }
		IMongoCollection<TimelineEventLog> TimelineLogger { get; }
		IMongoCollection<CTopic> TopicLookup { get; }
		IMongoCollection<TopicsModel> Topics {get; }
		IMongoCollection<CommentCorpus> CorpusComments { get; }
		IMongoCollection<MediaCorpus> CorpusMedia { get; }
		IMongoCollection<TopicCategory> TopicCategories { get; }
		IMongoCollection<HashtagsModel> Hashtags { get; }
		IMongoCollection<JobDto> Timeline { get; }
		IMongoCollection<MediasLib> MediaLibrary { get; }
		IMongoCollection<HashtagsLib> HashtagLibrary { get; }
		IMongoCollection<CaptionsLib> CaptionLibrary { get; }
		IMongoCollection<MessagesLib> MessagesLibrary { get; }

	}
}