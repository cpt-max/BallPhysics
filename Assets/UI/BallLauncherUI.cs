using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class BallLauncherUI : MonoBehaviour
{
    VisualElement root;

    void OnEnable()
    {
        var launcher = GetComponent<BallLauncher>();
        var uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

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

    void Update()
    {
        var launcher = GetComponent<BallLauncher>();
        var uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

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
}
