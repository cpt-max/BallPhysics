using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class BallLauncherUI : MonoBehaviour
{
    VisualElement root;

    void OnEnable()
    { 
        var uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;
        root.Q<Button>("reset").clicked += ResetClicked;

        InitSliders();
    }

    void Update()
    {
        var launcher = GetComponent<BallLauncher>();

        launcher.fireRate           = root.Q<Slider>("fireRate").value;
        launcher.speed              = root.Q<Slider>("speed").value;
        launcher.radius             = root.Q<Slider>("radius").value;
        launcher.density            = root.Q<Slider>("density").value;
        launcher.topSpin            = root.Q<Slider>("topSpin").value;
        launcher.sideSpin           = root.Q<Slider>("sideSpin").value;
        launcher.magnusLift         = root.Q<Slider>("magnusLift").value;
        launcher.elasticity         = root.Q<Slider>("elasticity").value;
        launcher.airDrag            = root.Q<Slider>("airDrag").value;
        launcher.rotAirDrag         = root.Q<Slider>("rotAirDrag").value;
        launcher.surfaceFriction    = root.Q<Slider>("surfaceFriction").value;
    }

    void InitSliders()
    {
        var launcher = GetComponent<BallLauncher>();

        InitSlider("fireRate", launcher.fireRate);
        InitSlider("speed", launcher.speed);
        InitSlider("radius", launcher.radius);
        InitSlider("density", launcher.density);
        InitSlider("topSpin", launcher.topSpin);
        InitSlider("sideSpin", launcher.sideSpin);
        InitSlider("magnusLift", launcher.magnusLift);
        InitSlider("elasticity", launcher.elasticity);
        InitSlider("airDrag", launcher.airDrag);
        InitSlider("rotAirDrag", launcher.rotAirDrag);
        InitSlider("surfaceFriction", launcher.surfaceFriction);
    }

    void InitSlider(string name, float value)
    {
        var slider = root.Q<Slider>(name);
        slider.value = value;

        try
        {
            var range = typeof(BallLauncher).GetField(name).GetCustomAttributes(true).OfType<RangeAttribute>().First();
            slider.lowValue = range.min;
            slider.highValue = range.max;
        } 
        catch 
        {
            Debug.Log("BallLauncherUI.InitSlider: no Range attribute for slider " + name);
        }
    }

    void ResetClicked()
    {
        var launcher = GetComponent<BallLauncher>();
        var bl = gameObject.AddComponent<BallLauncher>();

        launcher.fireRate = bl.fireRate;
        launcher.speed = bl.speed;
        launcher.radius = bl.radius;
        launcher.density = bl.density;
        launcher.topSpin = bl.topSpin;
        launcher.sideSpin = bl.sideSpin;
        launcher.magnusLift = bl.magnusLift;
        launcher.elasticity = bl.elasticity;
        launcher.airDrag = bl.airDrag;
        launcher.rotAirDrag = bl.rotAirDrag;
        launcher.surfaceFriction = bl.surfaceFriction;

        InitSliders();
        Destroy(bl);
    }
}
