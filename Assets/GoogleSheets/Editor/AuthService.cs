using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEditor;
using UnityEngine;

namespace UnityGoogleSheets
{
	public static class AuthService
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
				await Task.Run(() => AuthorizeAsync(credPath, googleTokenPath, cancellationToken), cancellationToken);
			}
			catch (Exception)
			{
				Debug.Log("Auth was aborted");
			}
			finally
			{
				Abort();
			}
		}

		public static void Abort()
		{
			_tokenSource?.Cancel();
			_tokenSource?.Dispose();
			_tokenSource = null;
		}

		static void AuthorizeAsync(string credPath, string tokenPath, CancellationToken cancellationToken)
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


			// Create Google Sheets API service.
			var service = new SheetsService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = "Google Sheets Unity",
			});

			// Define request parameters.
			String spreadsheetId = "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms";
			String range = "Class Data!A2:E";

			SpreadsheetsResource.ValuesResource.GetRequest request =
				service.Spreadsheets.Values.Get(spreadsheetId, range);
			

			// Prints the names and majors of students in a sample spreadsheet:
			// https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
			ValueRange response = request.Execute();

			var values = response.Values;
			if (values != null && values.Count > 0)
			{
				Debug.Log("Name, Major");
				foreach (var row in values)
				{
					// Print columns A and E, which correspond to indices 0 and 4.
					Debug.Log($"{row[0]}, {row[4]}");
				}
			}
			else
			{
				Debug.Log("No data found.");
			}
		}
	}
}