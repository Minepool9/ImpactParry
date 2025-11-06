using Configgy;
using Configgy.UI;
using UnityEngine;
using UnityEngine.UI;

public class RGBSliderConfig : ConfigValueElement<int[]>
{
    private Slider[] sliders = new Slider[3];
    private Image colorPreview;

    public RGBSliderConfig(int[] defaultValue) : base(defaultValue)
    {
        value = (int[])defaultValue.Clone();
    }

    protected override void BuildElementCore(RectTransform rect)
    {
        LoadValue();

        DynUI.ConfigUI.CreateElementSlot(rect, this, elementsDiv =>
        {
            Color[] channelColors = { Color.red, Color.green, Color.blue };

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                DynUI.Div(elementsDiv, sliderDiv =>
                {
                    DynUI.Slider(sliderDiv, slider =>
                    {
                        slider.minValue = 0;
                        slider.maxValue = 255;
                        slider.wholeNumbers = true;

                        if (value == null || value.Length < 3)
                            value = new int[3] { 255, 255, 255 };

                        slider.value = value[index];
                        sliders[index] = slider;

                        var fill = slider.fillRect?.GetComponent<Image>();
                        if (fill != null)
                            fill.color = channelColors[index] * (value[index] / 255f + 0.2f);

                        slider.onValueChanged.AddListener(v =>
                        {
                            value[index] = (int)v;
                            SetValue((int[])value.Clone());
                            UpdateSliderColors();
                            UpdateColorPreview();
                        });
                    });
                });
            }

            DynUI.Div(elementsDiv, previewDiv =>
            {
                RectTransform rt = previewDiv;
                rt.sizeDelta = new Vector2(40, 40);
                colorPreview = rt.gameObject.AddComponent<Image>();
                UpdateColorPreview();
            });
        });

        RefreshElementValue();
    }

    private void UpdateSliderColors()
    {
        if (sliders == null) return;

        Color[] channelColors = { Color.red, Color.green, Color.blue };
        for (int i = 0; i < 3; i++)
        {
            if (sliders[i] == null) continue;
            var fill = sliders[i].fillRect?.GetComponent<Image>();
            if (fill != null)
                fill.color = channelColors[i] * (value[i] / 255f + 0.2f);
        }
    }

    private void UpdateColorPreview()
    {
        if (colorPreview == null || value == null) return;

        colorPreview.color = new Color(
            value[0] / 255f,
            value[1] / 255f,
            value[2] / 255f
        );
    }

    protected override void RefreshElementValueCore()
    {
        if (value == null)
            LoadValue();

        for (int i = 0; i < 3; i++)
        {
            if (sliders[i] != null)
                sliders[i].SetValueWithoutNotify(value[i]);
        }

        UpdateSliderColors();
        UpdateColorPreview();
    }

    protected override void ResetValueCore()
    {
        value = (int[])DefaultValue.Clone();
        SetValue((int[])value.Clone());
        RefreshElementValue();
    }

    protected override int[] GetValueCore()
    {
        if (value == null)
            value = (int[])DefaultValue.Clone();
        return (int[])value.Clone();
    }
}
