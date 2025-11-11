using UnityEngine;
using UnityEngine.UI;
using PlantingMode;

public class NewPlantTemp : MonoBehaviour
{
    Button button;
    [Range(0, 2)]
    public int basicInfoID;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PlantingModeMgr.Instance.AddNewPlantButton(basicInfoID);
        });
    }
}
