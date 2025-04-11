using UnityEngine;
using UnityEngine.UI;

public class ChangeWorld : MonoBehaviour
{
    public BlobGenerator generator;

    public void NewWorld() 
    {
        float value = GetComponent<Slider>().value;
        generator.GenerateBlob((int)(value * 100));
    }
}


