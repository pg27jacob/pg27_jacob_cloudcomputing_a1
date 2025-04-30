using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
public class LoginUIHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;
    public Button signInButton;

    private void Start()
    {
        // Ensure the LoginManager instance exists
        if (LoginManager.Instance == null)
        {
            Debug.LogError("LoginManager not found in the scene!");
            enabled = false;
            return;
        }

        // Add a listener to the Sign In button's onClick event
        signInButton.onClick.AddListener(AttemptLogin);

    }


    void AttemptLogin()
    {
        if (usernameInputField != null && passwordInputField != null)
        {
            string username = usernameInputField.text;
            string password = passwordInputField.text;

            // Call the Login method on the LoginManager instance
            LoginManager.Instance.Login(username, password, LoginCompleteCallback);

            signInButton.interactable = false;
        }
        else
        {
            Debug.LogError("Username or Password input fields not assigned in the Inspector!");
        }
    }

    void LoginCompleteCallback(bool success)
    {
        signInButton.interactable = true;
        if (success)
        {
            Debug.Log("Login from Callback: Successful!");
        }
        else
        {
            Debug.LogError("Login from Callback: Failed!");
        }
    }
}