using UnityEngine;
using System;

public class MFilter : MonoBehaviour
{
    public enum FilterType { Allpass, LowPass, Notch, LowShelf, HighShelf };
    [SerializeField] FilterType _filterType;
    [Range(0.3f, 0.8f)] [SerializeField] double _q = 0.6;
    [Range(100f, 22000f)] [SerializeField] double _frequency = 250;
    [Range(-30, 0)] [SerializeField] double _gain;
    [Range(0f, 1f)] [SerializeField] float _vol = 1;

    double _a0 = 1, _b0 = 1, _b1, _b2, _a1, _a2;
    double _w0, _alpha, _A, _sampleRate, _cosW0, _sqrtAAlpha;

    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;
    }

    void QandFrequencyCalculation()
    {
        _w0 = 2f * Math.PI * (_frequency / _sampleRate);
        _cosW0 = Math.Cos(_w0);
        _alpha = Math.Sin(_w0) / (2 * _q);
    }

    void GainCalculation(FilterType type)
    {
        switch (type)
        {
            case FilterType.Allpass:
                return;
            case FilterType.LowPass:
                return;
            case FilterType.Notch:
                return;
            default:
                _A = Math.Pow(10, _gain / 40);
                _sqrtAAlpha = Math.Sqrt(_A) * _alpha;
                return;
        }
    }

    void CoefficientCalculation(FilterType type)
    {
        switch (type)
        {
            case FilterType.Allpass:
                _b0 = 1 - _alpha;
                _b1 = -2 * _cosW0;
                _b2 = 1 + _alpha;
                _a0 = 1 + _alpha;
                _a1 = -2 * _cosW0;
                _a2 = 1 - _alpha;
                break;

            case FilterType.LowPass:
                _b0 = (1 - _cosW0) / 2;
                _b1 = 1 - _cosW0;
                _b2 = (1 - _cosW0) / 2;
                _a0 = 1 + _alpha;
                _a1 = -2 * _cosW0;
                _a2 = 1 - _alpha;
                break;

            case FilterType.Notch:
                _b0 = 1;
                _b1 = -2 * _cosW0;
                _b2 = 1;
                _a0 = 1 + _alpha;
                _a1 = -2 * _cosW0;
                _a2 = 1 - _alpha;
                break;

            case FilterType.LowShelf:
                _b0 = _A * ((_A + 1) - (_A - 1) * _cosW0 + 2 * _sqrtAAlpha);
                _b1 = 2 * _A * ((_A - 1) - (_A + 1) * _cosW0);
                _b2 = _A * ((_A + 1) - (_A - 1) * _cosW0 - 2 * _sqrtAAlpha);
                _a0 = (_A + 1) + (_A - 1) * _cosW0 + 2 * _sqrtAAlpha;
                _a1 = -2 * ((_A - 1) + (_A + 1) * _cosW0);
                _a2 = (_A + 1) + (_A - 1) * _cosW0 - 2 * _sqrtAAlpha;
                break;

            case FilterType.HighShelf:
                _b0 = _A * ((_A + 1) + (_A - 1) * _cosW0 + 2 * _sqrtAAlpha);
                _b1 = -2 * _A * ((_A - 1) + (_A + 1) * _cosW0);
                _b2 = _A * ((_A + 1) + (_A - 1) * _cosW0 - 2 * _sqrtAAlpha);
                _a0 = (_A + 1) - (_A - 1) * _cosW0 + 2 * _sqrtAAlpha;
                _a1 = 2 * ((_A - 1) - (_A + 1) * _cosW0);
                _a2 = (_A + 1) - (_A - 1) * _cosW0 - 2 * _sqrtAAlpha;
                break;
        }
    }

    void Update()
    {
        QandFrequencyCalculation();
        GainCalculation(_filterType);
        CoefficientCalculation(_filterType);
    }

    struct DelayedSamples
    {
        public double za1, za2, zb1, zb2;
    }

    DelayedSamples[] _delayedSamples;

    void OnAudioFilterRead(float[] data, int channels)
    {

        double sampleIn, sampleOut;
        int count;

        if (_delayedSamples == null)
            _delayedSamples = new DelayedSamples[channels];

        for (int i = 0; i < data.Length; i++)
        {
            count = i % channels;

            if (count < _delayedSamples.Length)
            {
                DelayedSamples ds = _delayedSamples[count];
                sampleIn = data[i];
                sampleOut = 
                    (sampleIn * _b0 
                    + ds.zb1 * _b1 
                    + ds.zb2 * _b2 
                    - ds.za1 * _a1 
                    - ds.za2 * _a2) / _a0;

                data[i] = (float)sampleOut * _vol;

                _delayedSamples[count].zb2 = ds.zb1;
                _delayedSamples[count].zb1 = sampleIn;
                _delayedSamples[count].za2 = ds.za1;
                _delayedSamples[count].za1 = sampleOut;
            }
        }
    }
}
