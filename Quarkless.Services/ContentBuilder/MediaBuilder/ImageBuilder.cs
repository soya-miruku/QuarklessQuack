﻿using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Interfaces;
using Quarkless.Services.RequestBuilder.Consts;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ContentBuilderModels;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
using System.Threading.Tasks;
using Quarkless.Services.Extensions;

namespace Quarkless.Services.ContentBuilder.MediaBuilder
{
	public class ImageBuilder : IContent
	{
		private readonly ProfileModel _profile;
		private readonly UserStore _userSession;
		private readonly DateTime _executeTime;
		private readonly IContentBuilderManager _builder;
		private const int IMAGE_FETCH_LIMIT = 12;
		public ImageBuilder(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime)
		{
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
			_userSession = userSession;
		}
		public void Operate()
		{
			string exactSize = _profile.AdditionalConfigurations.PostSize;
			var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));
			var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0,_profile.Theme.Colors.Count));
			var topics = _builder.GetTopics(_userSession, _profile.TopicList,15).GetAwaiter().GetResult();	
			var topicSelect = topics.ElementAt(SecureRandom.Next(0,topics.Count));

			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(2).ToList();
			pickedSubsTopics.Add(topicSelect.TopicName);
			List<PostsModel> TotalResults = new List<PostsModel>();

			switch (_profile.AdditionalConfigurations.SearchTypes.ElementAtOrDefault(SecureRandom.Next(_profile.AdditionalConfigurations.SearchTypes.Count)))
			{
				case (int) SearchType.Google:
					var gres = _builder.GetGoogleImages(profileColor.Name, pickedSubsTopics, _profile.AdditionalConfigurations.Sites, IMAGE_FETCH_LIMIT,
						exactSize: exactSize);
					if(gres!=null)
						TotalResults.AddRange(gres);
					break;
				case (int) SearchType.Instagram:
					TotalResults.AddRange(_builder.GetMediaInstagram(_userSession, InstaMediaType.Image, pickedSubsTopics.ToList()));
					break;
				case (int) SearchType.Yandex:
					if(_profile.Theme.ImagesLike!=null && _profile.Theme.ImagesLike.Count > 0) { 
						List<GroupImagesAlike> groupImagesAlikes = new List<GroupImagesAlike>
						{
							_profile.Theme.ImagesLike.
							Where(s=>s.TopicGroup.ToLower() == topicSelect.TopicName.ToLower()).
							ElementAtOrDefault(SecureRandom.Next(_profile.Theme.ImagesLike.Count))
						};
						var yanres = _builder.GetYandexSimilarImages(groupImagesAlikes, IMAGE_FETCH_LIMIT*8);
						if(yanres!=null)
							TotalResults.AddRange(yanres);
					}
					break;
			}
			
			if(TotalResults.Count<=0) return;
			List<PostsModel> currentUsersMedia = _builder.GetUserMedia(_userSession,1).Take(5).ToList();

			List<byte[]> userMediaBytes = new List<byte[]>();
			Parallel.ForEach(currentUsersMedia.First().MediaData, act =>
			{
				userMediaBytes.Add(act.DownloadMedia());
			});

			List<byte[]> imagesBytes = new List<byte[]>();
			var resultSelect = TotalResults.ElementAtOrDefault(SecureRandom.Next(TotalResults.Count));

			Parallel.ForEach(resultSelect.MediaData.TakeAny(SecureRandom.Next(resultSelect.MediaData.Count)), s=> imagesBytes.Add(s?.DownloadMedia()));

			imagesBytes = userMediaBytes.Where(u=>u!=null)
				.RemoveDuplicateImages(imagesBytes,0.7)
				.ResizeManyToClosestAspectRatio()
				.Where(s=>s!=null).ToList();


			System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);
			
			var selectedImage = profileColorRGB.MostSimilarImage(imagesBytes.Where(_=>_!=null).ToList()).SharpenImage(4);

			if(selectedImage == null) return;
			var instaimage = new InstaImageUpload(){
				ImageBytes = selectedImage,
			};
			UploadPhotoModel uploadPhoto = new UploadPhotoModel
			{
				Caption = _builder.GenerateMediaInfo(topicSelect,_profile.Language),
				Image = instaimage,
				Location = location!=null ? new InstaLocationShort
				{
					Address = location.Address,
					Lat = location.Coordinates.Latitude,
					Lng = location.Coordinates.Longitude,
					Name = location.City
				} : null
			};
			RestModel restModel = new RestModel
			{
				User = _userSession,
				BaseUrl = UrlConstants.UploadPhoto,
				RequestType = RequestType.POST,
				JsonBody = JsonConvert.SerializeObject(uploadPhoto)
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}
	}
}

#region action code

//Action action = async()=> await _context.Media.UploadPhotoAsync(new InstaImageUpload(),"caption",new InstaLocationShort());

/*_builder.AddToTimeline(restModel,_executeTime);

var actionFunc = new Func<Task<IResult<InstaMedia>>>(async()=>
{
	return await _context.Media.UploadPhotoAsync(instaimage,"chaos");
});

_builder.AddActionToTimeLine(actionFunc,_executeTime);
*/

#endregion