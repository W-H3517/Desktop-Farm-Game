using UnityEngine;
using UnityEngine.UI;

public class NewPlantTemp : MonoBehaviour
{
    Button button;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            StartScript.Instance.AddNewPlant();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
