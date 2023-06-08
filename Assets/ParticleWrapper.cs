using UnityEngine;

public class ParticleWrapper
{

    ParticleSystem particleSystem;

    float startSpeed;
    float rate;

    public ParticleWrapper(ParticleSystem system) {
        particleSystem = system;

        // Store default values
        ParticleSystem.MainModule main = particleSystem.main;
        startSpeed = main.startSpeed.constant;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        rate = emission.rateOverTime.constant;
    }

    public void SetEnabled(bool isEnabled) {
        ParticleSystem.EmissionModule module = particleSystem.emission;
        module.enabled = isEnabled;
    }

    public void SetPower(float power) {
        ParticleSystem.MainModule main = particleSystem.main;
        main.startSpeed = startSpeed * power;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = rate * power * power;
    }
}
