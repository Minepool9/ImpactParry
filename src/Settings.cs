namespace ImpactParry;

using Configgy;

public class Settings
{
    [Configgable]
    public static ConfigToggle enabled = new(true);

    [Configgable("Shader", "Posterize Levels")]
    public static FloatSlider PosterizeLevels = new(5.6f, 2f, 16f, 0.1f);

    [Configgable("Shader", "Posterize Strength")]
    public static FloatSlider PosterizeStrength = new(1f, 0f, 1f, 0.1f);

    [Configgable("Shader", "Shading Blend")]
    public static FloatSlider ShadingBlend = new(0f, 0f, 1f, 0.1f);

    [Configgable("Shader", "Contrast")]
    public static FloatSlider Contrast = new(1.3f, 0.1f, 3f, 0.1f);

    [Configgable("Shader", "Brightness")]
    public static FloatSlider Brightness = new(2.25f, 0.1f, 10f, 0.1f);

    [Configgable("Shader", "White Tint")]
    public static RGBSliderConfig WhiteTint = new([ 255, 255, 255 ]);

    [Configgable("Shader", "Black Tint")]
    public static RGBSliderConfig BlackTint = new([ 0, 0, 0 ]);

    // New toggle to switch between slider vs input field mode
    [Configgable("Shader", "Use Individual Inputs")]
    public static ConfigToggle UseIndividualInputs = new(false);

    // Individual input fields for white and black tints
    [Configgable("Shader/Individual/White", "White R")] public static ConfigInputField<int> WhiteR = new(255);
    [Configgable("Shader/Individual/White", "White G")] public static ConfigInputField<int> WhiteG = new(255);
    [Configgable("Shader/Individual/White", "White B")] public static ConfigInputField<int> WhiteB = new(255);

    [Configgable("Shader/Individual/Black", "Black R")] public static ConfigInputField<int> BlackR = new(0);
    [Configgable("Shader/Individual/Black", "Black G")] public static ConfigInputField<int> BlackG = new(0);
    [Configgable("Shader/Individual/Black", "Black B")] public static ConfigInputField<int> BlackB = new(0);
}
