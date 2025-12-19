using UnityEngine;

public class GrassObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = Vector3.up * DataMgr.Instance.WorldBottomLocation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
