using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

public class LoginManager : MonoBehaviour
{
    private const string loginUrl = "http://localhost:3000/login"; // Change to your server's address

    public static LoginManager Instance;

    public string AuthToken { get; private set; } // Public property to access the token

    public bool IsLoggedIn { get; private set; } // Public property to check login status

    public delegate void LoginCallback(bool success); // Define a delegate type for the callback
    public event LoginCallback OnLogin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scenes
            // Load token from PlayerPrefs on startup
            AuthToken = PlayerPrefs.GetString("AuthToken", "");
            IsLoggedIn = !string.IsNullOrEmpty(AuthToken); // Set initial login state
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Login(string username, string password, Action<bool> onLoginComplete = null) // Optional callback
    {
        StartCoroutine(SendLoginRequest(username, password, onLoginComplete));
    }

    IEnumerator SendLoginRequest(string username, string password, Action<bool> onLoginComplete)
    {
        LoginCredentials credentials = new LoginCredentials { username = username, password = password };
        string jsonPayload = JsonUtility.ToJson(credentials);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest www = new UnityWebRequest(loginUrl, "POST"))
        {
            UploadHandlerRaw uploadHandler = new UploadHandlerRaw(bodyRaw);
            uploadHandler.contentType = "application/json";
            www.uploadHandler = uploadHandler;
            www.downloadHandler = new DownloadHandlerBuffer();

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Login failed: " + www.error);
                OnLogin?.Invoke(false);
                onLoginComplete?.Invoke(false);
            }
            else
            {
                Debug.Log("Login successful: " + www.downloadHandler.text);
                try
                {
                    LoginResponse response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                    AuthToken = response.token;
                    IsLoggedIn = true;
                    Debug.Log("Received Token: " + AuthToken);
                    PlayerPrefs.SetString("AuthToken", AuthToken);
                    OnLogin?.Invoke(true);
                    onLoginComplete?.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing login response: " + e.Message);
                    OnLogin?.Invoke(false);
                    onLoginComplete?.Invoke(false);
                }
            }
        }
    }

    // Helper class to structure the login credentials as JSON
    [System.Serializable]
    private class LoginCredentials
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }

    public void Logout()
    {
        AuthToken = "";
        IsLoggedIn = false;
        PlayerPrefs.DeleteKey("AuthToken");
    }

}