using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using UnityEditor;
using UnityEngine;

namespace UnityGoogleSheets
{
	public static class SheetService
	{
		static string GOOGLE_TOKEN_PATH => Path.Combine(Application.persistentDataPath, "google_sheets_token.json");

		public static bool CredentialExist
		{
			get
			{
				return AssetDatabase.FindAssets("credentials", new[] {"Assets"})
					.Length == 1;
			}
		}

		static string CredentialPath
		{
			get
			{
				var c = AssetDatabase.FindAssets("credentials", new[] {"Assets"});

				return AssetDatabase.GUIDToAssetPath(c[0]);
			}
		}

		public static bool Authorized => Directory.Exists(GOOGLE_TOKEN_PATH)
		                                 && Directory.GetFiles(GOOGLE_TOKEN_PATH).Length != 0;


		static CancellationTokenSource _tokenSource;

		public static bool IsProcessingAuth => _tokenSource != null;

		public static void SignOut()
		{
			Directory.Delete(GOOGLE_TOKEN_PATH, true);
		}

		public static async void SignIn()
		{
			if (IsProcessingAuth) return;

			try
			{
				_tokenSource?.Cancel();
				_tokenSource?.Dispose();
				_tokenSource = new CancellationTokenSource();
				_tokenSource.CancelAfter(new TimeSpan(0, 3, 0));
				var cancellationToken = _tokenSource.Token;
				var credPath = CredentialPath;
				var googleTokenPath = GOOGLE_TOKEN_PATH;
				await Task.Run(() => Authorize(credPath, googleTokenPath, cancellationToken), cancellationToken);
			}
			catch (Exception)
			{
				Debug.Log("Auth was aborted");
			}
			finally
			{
				AbortAuth();
			}
		}

		public static void AbortAuth()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}

#pragma warning disable 1998
		static async Task<UserCredential> Authorize(string credPath, string tokenPath, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			UserCredential credential;
			string[] scopes = {SheetsService.Scope.SpreadsheetsReadonly};


			using (var stream =
				new FileStream(credPath, FileMode.Open, FileAccess.Read))
			{
				credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.Load(stream).Secrets,
					scopes,
					"user",
					cancellationToken,
					new FileDataStore(tokenPath, true)).Result;
			}

			return credential;
		}

		public static async Task<List<List<string>>> GetTableValues(string spreadsheetId, string range = "A1")
		{
			var service = await CreateService();
			var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
			var values = request.Execute().Values;

			if (values == null || values.Count <= 0) return null;

			var res = new List<List<string>>();
			foreach (var iList in values)
			{
				var l = new List<string>();
				res.Add(l);
				l.AddRange(iList.Select(val => val.ToString()));
			}

			return res;
		

		}

		static async Task<SheetsService> CreateService()
		{
			var conSrc = new CancellationTokenSource();
			conSrc.CancelAfter(new TimeSpan(0, 1, 0));

			var service = new BaseClientService.Initializer()
			{
				HttpClientInitializer = await Authorize(CredentialPath, GOOGLE_TOKEN_PATH, conSrc.Token),
				ApplicationName = "Google Sheets Unity",
			};

			return new SheetsService(service);
		}
	}
}