using System.Collections.Generic;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    public UnityToAPI toAPI;
    public ParticleSystem burnerFireParticles;
    public  List<HeatZone> heatZone;
    public int activeZones = 0;
    public bool ovenOff;
    public void RegisterZoneStateChange(bool isOn)
    {
        if (isOn)
        {
            activeZones++;
            if (activeZones == 1)
            {
                Debug.Log(" First heat zone activated. Submitting API query...");
                toAPI.queryText = "I turned on the stove in the game. What now?";
                
                toAPI.SubmitQuery();
                Debug.Log(toAPI.queryText);

                if (burnerFireParticles != null)
                    burnerFireParticles.Play();
            }
        }
        else
        {
            activeZones = Mathf.Max(0, activeZones - 1);
            if (activeZones == 0)
            {
                Debug.Log(" All heat zones turned off. Stove is now OFF.");
                toAPI.queryText = "I turned off the stove in the game. What now?";
                 
                 toAPI.SubmitQuery();
                Debug.Log(toAPI.queryText);
                if (burnerFireParticles != null)
                    burnerFireParticles.Stop();
            }
        }
    }
}
