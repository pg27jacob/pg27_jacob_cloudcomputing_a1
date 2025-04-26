using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

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

    IEnumerator SendLoginRequest(string username, string password, Action<bool> onLoginComplete) // Optional callback
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(loginUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Login failed: " + www.error);
                OnLogin?.Invoke(false); // Notify about the login
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
                    // Store authToken securely (PlayerPrefs is NOT ideal for sensitive data, but fine for this example)
                    PlayerPrefs.SetString("AuthToken", AuthToken);
                    OnLogin?.Invoke(true); //changed
                    onLoginComplete?.Invoke(true);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error parsing login response: " + e.Message);
                    OnLogin?.Invoke(false); // Notify about the login failure
                    onLoginComplete?.Invoke(false);
                }
            }
        }
    }

    public void Logout()
    {
        AuthToken = "";
        IsLoggedIn = false;
        PlayerPrefs.DeleteKey("AuthToken");
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
    }
}