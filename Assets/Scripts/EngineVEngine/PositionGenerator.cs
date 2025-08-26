using System.IO;
using UnityEngine;

public class PositionGenerator : MonoBehaviour
{
    public static PositionGenerator instance;

    [SerializeField] private string filePath;
    [SerializeField] private int maxIndex;
    [Space, SerializeField] private bool test;

    void Awake()
    {
        instance = this;
    }

    public string GetRandomFen()
    {
        int randomIndex = Random.Range(0, maxIndex);
        Debug.Log("Picked Fen at index " + randomIndex);

        StreamReader reader = File.OpenText(filePath);
        for (int i = 0; i < randomIndex; i++)
        {
            reader.ReadLine(); //Skip these lines
        }

        return reader.ReadLine(); //Return the one we arrived at
    }

    void OnValidate()
    {
        if (test)
        {
            test = false;
            Debug.Log(GetRandomFen());
        }
    }
}