using FGear;
using UnityEngine;

public class CarEnforcer : MonoBehaviour
{
    [SerializeField] private Vehicle vehicle;

    private void Awake()
    {
        LoadData();
    }

    private void EnforceCar()
    {
        var acceleration = vehicle.getEngine().getTorqueScale();
        var maxSpeed = vehicle.getTransmission().getFinalGearRatio();

        if (acceleration <= 2f)
        {
            acceleration += 0.1f;
        }

        if (maxSpeed >= 2.2f)
        {
            maxSpeed -= 0.3f;
        }

        vehicle.getEngine().setTorqueScale(acceleration);
        vehicle.getTransmission().setFinalGearRatio(maxSpeed);

        PlayerPrefs.SetFloat("carAcceleration", vehicle.getEngine().getTorqueScale());
        PlayerPrefs.SetFloat("carMaxSpeed", vehicle.getTransmission().getFinalGearRatio());
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        var acceleration = PlayerPrefs.GetFloat("carAcceleration");
        var maxSpeed = PlayerPrefs.GetFloat("carMaxSpeed");

        if (!PlayerPrefs.HasKey("carAcceleration"))
        {
            PlayerPrefs.SetFloat("carAcceleration", 1f);
        }

        if (!PlayerPrefs.HasKey("carMaxSpeed"))
        {
            PlayerPrefs.SetFloat("carMaxSpeed", 4f);
        }

        if (acceleration > 1.9f)
        {
            acceleration = 2f;
        }

        if (maxSpeed < 2.3f)
        {
            maxSpeed = 2.2f;
        }

        vehicle.getEngine().setTorqueScale(acceleration);
        vehicle.getTransmission().setFinalGearRatio(maxSpeed);
    }
}