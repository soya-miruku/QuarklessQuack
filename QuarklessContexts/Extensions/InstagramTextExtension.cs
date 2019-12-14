﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuarklessContexts.Extensions
{
	public static class InstagramTextExtension
	{
		public static string MatchOnlyHashtags => @"(#\w*)"; 
		public static string MatchOnlyMentions => @"(@\w*)";
		public static string MatchOnlyCurrency => @"(\w*[$|£|€|¥|]\w*)";
		public static string MatchOnlyMultipleSpaces = @"(\s{2,})";
		public static string MatchAnyVerticalSeparation = @"^[^a-z|^A-Z]$";
		public static string MatchAnyHorizontalSeparation = @"^[^\w|\s|_]{2,}$";
		public static string MatchOnlyNonWordsChars => @"[^a-z|^A-Z|^\s|^\.]{3,}";
		public static string MatchOnlyPhoneNumbers => @"\d{7,}";
		public static string MatchOnlyWebAddresses => @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:\/?#[\]@!\$&'\(\)\*\+,;=.]+$";

		public static string RemoveHorizontalSeparationFromText(this string text) =>
			Regex.Replace(text, MatchAnyHorizontalSeparation, " ");
		public static string RemoveVerticalSeparationFromText(this string text) =>
			Regex.Replace(text, MatchAnyVerticalSeparation, " ");
		public static string RemovePhoneNumbersFromText(this string text) =>
			Regex.Replace(text, MatchOnlyPhoneNumbers, " ");
		public static string RemoveWebAddressesFromText(this string text) =>
			Regex.Replace(text, MatchOnlyWebAddresses, " ");
		public static string RemoveNonWordsFromText(this string text) =>
			Regex.Replace(text, MatchOnlyNonWordsChars, "");
		public static IEnumerable<string> FilterHashtags(this string text) => 
			Regex.Matches(text, MatchOnlyHashtags).Select(s => s.Value);
		public static string RemoveHashtagsFromText(this string text) => 
			Regex.Replace(text, MatchOnlyHashtags, " ");
		public static IEnumerable<string> FilterMentions(this string text) =>
			Regex.Matches(text, MatchOnlyMentions).Select(s => s.Value);
		public static string RemoveMentionsFromText(this string text) =>
			Regex.Replace(text, MatchOnlyMentions, " ");
		public static IEnumerable<string> FilterCurrency(this string text) =>
			Regex.Matches(text, MatchOnlyCurrency).Select(s => s.Value);
		public static string RemoveCurrencyFromText(this string text) =>
			Regex.Replace(text, MatchOnlyCurrency, " ");
		public static string RemoveLargeSpacesInText(this string text) =>
			Regex.Replace(text, MatchOnlyMultipleSpaces, " ");
	}
}
