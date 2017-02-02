using UnityEngine;
using System.Collections;

public class GunManager : MonoBehaviour {

    public static GunManager Instance
    {
        get
        {
            if(Instance == null)
            {
                GameObject go = new GameObject("GunManager");
                go.AddComponent<GunManager>();
            }
            return instance;
        }     
    }

    private static GunManager instance = null;
    public enum GunState {  idle, walk, shoot};

}
