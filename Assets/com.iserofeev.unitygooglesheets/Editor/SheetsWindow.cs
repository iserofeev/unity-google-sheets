using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityGoogleSheets
{
	public class SheetsWindow : EditorWindow
	{
		[MenuItem("Tools/GoogleSheets Panel")]
		static void Init()
		{
			var window = GetWindow<SheetsWindow>("GoogleSheets Panel");
			window.minSize = new Vector2(300, 400);
			window.Show();

		}
		


		void OnGUI()
		{
			if (!SheetService.CredentialExist)
			{
				EditorGUILayout.LabelField("Credential file not found, put in anywhere in Assets");
				GetCredentialBinding = GUILayout.Button("Enable the Google Sheets API");
				return;
			}

			if (!SheetService.Authorized && !SheetService.IsProcessingAuth)
			{
				SignInBinding = GUILayout.Button("Sign In");
			}

			if (SheetService.Authorized)
			{
				EditorGUILayout.LabelField("Authorized");
				SignOutBinding = GUILayout.Button("Sign Out");


			}

			if (SheetService.IsProcessingAuth)
			{
				EditorGUILayout.LabelField("Processing");
				AbortAuth = GUILayout.Button("Abort");
			}
		}

		#region Bindings
		static bool SignInBinding
		{
			set
			{
				if(value) SheetService.SignIn();
			}
		}
		
		static bool SignOutBinding
		{
			set
			{
				if(value) SheetService.SignOut();
			}
		}

		static bool AbortAuth
		{
			set
			{
				if(value) SheetService.AbortAuth();
			}
		}

		static bool GetCredentialBinding
		{
			set
			{
				if(value) Application.OpenURL("https://developers.google.com/sheets/api/quickstart/dotnet");
			}
		}


		#endregion

	}
}