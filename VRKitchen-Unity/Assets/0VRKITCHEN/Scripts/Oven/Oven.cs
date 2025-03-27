using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenController : MonoBehaviour
{
    public bool isOvenOn = false;
    public UnityToAPI toAPI;

    // 🔥 Reference to the fire particle system
    public ParticleSystem burnerFireParticles; // 👈 Drag the Particle System here in Unity

    public void ToggleOven()
    {
        isOvenOn = !isOvenOn;

        if (isOvenOn)
        {
            Debug.Log("Oven is ON");
            toAPI.queryText = "Stove is turned on, what are the next steps in the game while making the dish?";
            Debug.Log("Query Submitted");
            toAPI.SubmitQuery();

            if (burnerFireParticles != null)
                burnerFireParticles.Play(); // 🔥 Start the fire particles
        }
        else
        {
            Debug.Log("Oven is OFF");
            toAPI.queryText = "Stove is off, what's the next step in the game?";
            //toAPI.SubmitQuery();

            if (burnerFireParticles != null)
                burnerFireParticles.Stop(); // ⚫ Stop the fire particles
        }
    }
}