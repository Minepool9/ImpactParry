using Configgy;
using UnityEngine;

public class MyCoolBluuToon
{
    [Configgable]
    public static ConfigToggle enabled = new ConfigToggle(true);

    [Configgable("Shader", "Posterize Levels")]
    public static FloatSlider PosterizeLevels = new FloatSlider(5.6f, 2f, 16f, 0.1f);

    [Configgable("Shader", "Posterize Strength")]
    public static FloatSlider PosterizeStrength = new FloatSlider(1f, 0f, 1f, 0.1f);

    [Configgable("Shader", "Shading Blend")]
    public static FloatSlider ShadingBlend = new FloatSlider(0f, 0f, 1f, 0.1f);

    [Configgable("Shader", "Contrast")]
    public static FloatSlider Contrast = new FloatSlider(1.3f, 0.1f, 3f, 0.1f);

    [Configgable("Shader", "Brightness")]
    public static FloatSlider Brightness = new FloatSlider(2.25f, 0.1f, 10f, 0.1f);

    [Configgable("Shader", "White Tint")]
    public static RGBSliderConfig WhiteTint = new RGBSliderConfig(new int[] { 255, 255, 255 });

    [Configgable("Shader", "Black Tint")]
    public static RGBSliderConfig BlackTint = new RGBSliderConfig(new int[] { 0, 0, 0 });

    // New toggle to switch between slider vs input field mode
    [Configgable("Shader", "Use Individual Inputs")]
    public static ConfigToggle UseIndividualInputs = new ConfigToggle(false);

    // Individual input fields for white and black tints
    [Configgable("Shader/Individual/White", "White R")] public static ConfigInputField<int> WhiteR = new ConfigInputField<int>(255);
    [Configgable("Shader/Individual/White", "White G")] public static ConfigInputField<int> WhiteG = new ConfigInputField<int>(255);
    [Configgable("Shader/Individual/White", "White B")] public static ConfigInputField<int> WhiteB = new ConfigInputField<int>(255);

    [Configgable("Shader/Individual/Black", "Black R")] public static ConfigInputField<int> BlackR = new ConfigInputField<int>(0);
    [Configgable("Shader/Individual/Black", "Black G")] public static ConfigInputField<int> BlackG = new ConfigInputField<int>(0);
    [Configgable("Shader/Individual/Black", "Black B")] public static ConfigInputField<int> BlackB = new ConfigInputField<int>(0);
}
