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
			if (!AuthService.CredentialExist)
			{
				EditorGUILayout.LabelField("Credential file not found, put in anywhere in Assets");
				GUILayout.Box("Get it on the button");
				
				GetCredentialBinding = GUILayout.Button("Enable the Google Sheets API");
				
				
				return;
			}

			if (!AuthService.Authorized && !AuthService.IsProcessingAuth)
			{
				SignInBinding = GUILayout.Button("Sign In");
			}

			if (AuthService.Authorized)
			{
				EditorGUILayout.LabelField("Authorized");
				SignOutBinding = GUILayout.Button("Sign Out");


			}

			if (AuthService.IsProcessingAuth)
			{
				EditorGUILayout.LabelField("Processing");
				AbortAuth = GUILayout.Button("Abort");
			}
			else
			{
				EditorGUILayout.LabelField("NON Processing");
			}
		}

		#region Bindings
		static bool SignInBinding
		{
			set
			{
				if(value) AuthService.SignIn();
			}
		}
		
		static bool SignOutBinding
		{
			set
			{
				if(value) AuthService.SignOut();
			}
		}

		static bool AbortAuth
		{
			set
			{
				if(value) AuthService.Abort();
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