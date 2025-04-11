using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Camera targetCamera;

    private int _initialScore = 0;
    private int _score;

    public TextMeshProUGUI UItext;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(SetOriginalScore());
    }

    private IEnumerator SetOriginalScore()
    {
        yield return CountPixels();
        _initialScore = _score;
        UItext.text = "0";
    }

    public void CountNonWhitePixels()
    {
        StartCoroutine(CountPixels());
    }

    private IEnumerator CountPixels()
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        Rect region = new Rect(0, 0, width, height);
        texture.ReadPixels(region, 0, 0);
        texture.Apply();

        Color[] pixels = texture.GetPixels();

        int nonWhiteCount = 0;
        foreach (Color pixel in pixels)
        {
            if (!IsWhite(pixel))
            {
                nonWhiteCount++;
            }
        }

        _score = nonWhiteCount - _initialScore;
        UItext.text = _score.ToString();
    }

    private bool IsWhite(Color color)
    {
        return color.r > 0.9f && color.g > 0.9f && color.b > 0.9f;
    }
}
