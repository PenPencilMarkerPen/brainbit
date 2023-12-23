using NeuroSDK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using SignalMath;
using UnityEngine.Android;
public class Tess : MonoBehaviour
{
    Scanner scanner = new Scanner(SensorFamily.SensorLEBrainBit);
    BrainBitSensor sensor;
    private bool _isFounded = false;
    [SerializeField] private TextMeshProUGUI textPower;
    //int samplingFrequency = 250;
    //MathLibSetting mls;
    //ArtifactDetectSetting ads;
    //ShortArtifactDetectSetting sads;
    //MentalAndSpectralSetting mss;
    //EegEmotionalMath math;
    int kuk;
    //bool calibrationFinished=false;
    //int calibrationProgress=0;
    private void Awake()
    {
#if UNITY_ANDROID
        if (SystemInfo.operatingSystem.Contains("31") ||
            SystemInfo.operatingSystem.Contains("32") ||
            SystemInfo.operatingSystem.Contains("33"))
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        }
        else
        {
            Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
            Permission.RequestUserPermission("android.permission.ACCESS_COARSE_LOCATION");
        }
#endif
        scanner.EventSensorsChanged += Scanner_Founded;
    }
    private void Start()
    {
        scanner.Start();

    }
    private void Update()
    {
        //Debug.Log(calibrationFinished);
        //Debug.Log(calibrationProgress);
        if (kuk != 0)
        {
            textPower.text = kuk + "";
            //Debug.Log(math);
        }

    }
    private void OnDestroy()
    {
        scanner.EventSensorsChanged -= Scanner_Founded;
        sensor.EventBatteryChanged -= EventBatteryChanged;
        scanner.Dispose();
    }

    private void Scanner_Founded(IScanner scanner, IReadOnlyList<SensorInfo> sensors)
    {

        foreach (SensorInfo sensorInfo in sensors)
        {
            Debug.Log(sensorInfo.SerialNumber);
            if (sensorInfo.SerialNumber == "132510")
            {
                sensor = scanner.CreateSensor(sensorInfo) as BrainBitSensor;
                var commands = sensor.Commands;
                foreach (var command in commands)
                {
                    Debug.Log(command);
                }
                scanner.Stop();
                scanner.Dispose();
                sensor.EventBatteryChanged += EventBatteryChanged;
                Debug.Log("what");
                //Progruz();
                //math.StartCalibration();
                //calibrationFinished = math.CalibrationFinished();
                // and calibration progress
                //calibrationProgress = math.GetCallibrationPercents();

            }
        }

    }

    private void Gdh(ISensor sensor, int battPower)
    {
        textPower.text = battPower.ToString();
    }
    private void EventBatteryChanged(ISensor sensor, int battPower)
    {

        kuk = battPower;
        Debug.Log("Power: " + battPower);
        //textPower.text = battPower.ToString();
    }

    //    public void Progruz()
    //    {
    //        mls = new MathLibSetting
    //        {
    //            sampling_rate = samplingFrequency,
    //            process_win_freq = 25,
    //            n_first_sec_skipped = 6,
    //            fft_window = samplingFrequency * 2,
    //            bipolar_mode = true,
    //            channels_number = 4,
    //            channel_for_analysis = 0
    //        };
    //        ads = new ArtifactDetectSetting
    //        {
    //            art_bord = 110,
    //            allowed_percent_artpoints = 70,
    //            raw_betap_limit = 800_000,
    //            total_pow_border = 3 * 117,
    //            global_artwin_sec = 4,
    //            spect_art_by_totalp = false,
    //            num_wins_for_quality_avg = 100,
    //            hanning_win_spectrum = false,
    //            hamming_win_spectrum = true
    //        };
    //        sads = new ShortArtifactDetectSetting
    //        {
    //            ampl_art_detect_win_size = 200,
    //            ampl_art_zerod_area = 200,
    //            ampl_art_extremum_border = 25
    //        };
    //        mss = new MentalAndSpectralSetting
    //        {
    //            n_sec_for_averaging = 2,
    //            n_sec_for_instant_estimation = 2
    //        };
    //        math = new EegEmotionalMath(mls, ads, sads, mss);
    //    }
    //}
}