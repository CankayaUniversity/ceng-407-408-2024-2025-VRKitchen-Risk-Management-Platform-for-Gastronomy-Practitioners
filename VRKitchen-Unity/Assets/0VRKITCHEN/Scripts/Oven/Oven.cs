using System.Collections.Generic;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    public UnityToAPI toAPI;
    public ParticleSystem burnerFireParticles;
    public  List<HeatZone> heatZone;
    public int activeZones = 0;

    public void RegisterZoneStateChange(bool isOn)
    {
        if (isOn)
        {
            activeZones++;
            if (activeZones == 1)
            {
                Debug.Log(" First heat zone activated. Submitting API query...");
                toAPI.queryText = "I turned on the stove, What is the next steps? Give me the first one. Without providing a entrance, just step.";
                toAPI.SubmitQuery();

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
                toAPI.queryText = "Stove is off, what's the next step in the game? One step.";
                 
                 toAPI.SubmitQuery();

                if (burnerFireParticles != null)
                    burnerFireParticles.Stop();
            }
        }
    }
}
