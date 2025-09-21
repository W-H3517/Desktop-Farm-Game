using UnityEngine;
using UnityEngine.UI;
using PlantingMode;

public class NewPlantTemp : MonoBehaviour
{
    Button button;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PlantingModeMgr.Instance.AddNewPlant();
        });
    }
}
