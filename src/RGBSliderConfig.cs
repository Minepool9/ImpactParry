using Configgy;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

public class RGBSliderConfig : ConfigValueElement<int[]>
{
    public int[] ValueArray = new int[3];
    private Slider[] sliders = new Slider[3];
    private Image colorPreview;

    public RGBSliderConfig(int[] defaultValue) : base(defaultValue)
    {
        for (int i = 0; i < 3; i++)
            ValueArray[i] = defaultValue[i];
    }

    protected override void BuildElementCore(RectTransform rect)
    {
        DynUI.ConfigUI.CreateElementSlot(rect, this, (elementsDiv) =>
        {
            Color[] channelColors = { Color.red, Color.green, Color.blue };

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                DynUI.Div(elementsDiv, (sliderDiv) =>
                {
                    DynUI.Slider(sliderDiv, (slider) =>
                    {
                        slider.minValue = 0;
                        slider.maxValue = 255;
                        slider.wholeNumbers = true;
                        slider.value = ValueArray[index];
                        sliders[index] = slider;

                        var fill = slider.fillRect?.GetComponent<Image>();
                        if (fill != null)
                            fill.color = channelColors[index] * (ValueArray[index] / 255f + 0.2f);

                        slider.onValueChanged.AddListener((v) =>
                        {
                            ValueArray[index] = (int)v;
                            UpdateSliderColors();
                            UpdateColorPreview();
                            SetValue(GetValue()); // Mark as dirty for saving
                        });
                    });
                });
            }

            DynUI.Div(elementsDiv, (previewDiv) =>
            {
                RectTransform rt = previewDiv;
                rt.sizeDelta = new Vector2(40, 40);
                colorPreview = rt.gameObject.AddComponent<Image>();
                UpdateColorPreview();
            });
        });
    }

    private void UpdateSliderColors()
    {
        Color[] channelColors = { Color.red, Color.green, Color.blue };
        for (int i = 0; i < 3; i++)
        {
            var fill = sliders[i].fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = channelColors[i] * (ValueArray[i] / 255f + 0.2f);
        }
    }

    private void UpdateColorPreview()
    {
        if (colorPreview != null)
        {
            colorPreview.color = new Color(
                ValueArray[0] / 255f,
                ValueArray[1] / 255f,
                ValueArray[2] / 255f
            );
        }
    }

    protected override void RefreshElementValueCore()
    {
        for (int i = 0; i < 3; i++)
        {
            if (sliders[i] != null)
                sliders[i].value = ValueArray[i];
        }
        UpdateSliderColors();
        UpdateColorPreview();
    }

    protected override void ResetValueCore()
    {
        for (int i = 0; i < 3; i++)
            ValueArray[i] = DefaultValue[i];

        RefreshElementValue();
        SetValue(GetValue());
    }

    public void ResetRGBManually()
    {
        ResetValueCore();
    }

    protected override int[] GetValueCore()
    {
        return new int[] { ValueArray[0], ValueArray[1], ValueArray[2] };
    }
}
