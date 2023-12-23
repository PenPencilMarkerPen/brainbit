using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using TMPro;
using NeuroSDK;
using SignalMath;
using UnityEngine.Android;
public class Danko : MonoBehaviour
{
    Scanner scanner = new Scanner(SensorFamily.SensorLEBrainBit);
    private Thread sensorThread;
    private ISensor sensor;
    private SensorInfo sensorInfo1;
    [SerializeField] private TextMeshProUGUI duka;
    int kuk=0;
    //rainBitSensor sensor;
    int samplingFrequency = 250;
    //MathLibSetting mls;
    //ArtifactDetectSetting ads;
    //ShortArtifactDetectSetting sads;
    //MentalAndSpectralSetting mss;
    EegEmotionalMath math;
    int calibrationProgress;
    bool calibrationFinished;
    bool odin = false;
    bool dwa = false;
    // Start is called before the first frame update
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
    void Start()
    {
        scanner.Start();
        
    }

    void Update()
    {
        if (kuk != 0)
        {
            duka.text = kuk.ToString();
        }
    }
    private void Scanner_Founded(IScanner scanner, IReadOnlyList<SensorInfo> sensors)
    {
        foreach (SensorInfo sensorInfo in sensors)
        {
            Debug.Log(sensorInfo.Name + ": " + sensorInfo.Address + " : " + sensorInfo.SerialNumber);
            if (sensorInfo.SerialNumber == "132510"&&odin==false)
            {
                //odin = true
                sensorInfo1 = sensorInfo;
                sensorThread = new Thread(InitializeSensor);
                sensorThread.Start();
                scanner.Stop();
                Debug.Log("ff");
                
            }
        }
    }
    private void InitializeSensor()
    {
        // Инициализация сенсора и прочая логика
        //Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        sensor = scanner.CreateSensor(sensorInfo1);
        sensor.EventBatteryChanged += EventBatteryChanged;
        (sensor as BrainBitSensor).ExecCommand(SensorCommand.CommandStartSignal);
        (sensor as BrainBitSensor).EventBrainBitSignalDataRecived += Danko_EventBrainBitSignalDataRecived;
        Debug.Log("ff");
        Progruz();


    }

    private void Danko_EventBrainBitSignalDataRecived(ISensor sensor, BrainBitSignalData[] data)
    {
       

        var bipolarSamples = new RawChannels[data.Length];

        for (var i = 0; i < data.Length; i++)
        {
            bipolarSamples[i].LeftBipolar = data[i].T3 - data[i].O1;
            bipolarSamples[i].RightBipolar = data[i].T4 - data[i].O2;
        }

        try
        {
            math.PushData(bipolarSamples);
            math.ProcessDataArr();

            bool calibrationFinished = math.CalibrationFinished();
            if (calibrationFinished) { 
            MindData[] mentalData = math.ReadMentalDataArr();
                Debug.Log(mentalData[0].RelAttention);
                Debug.Log(mentalData[0].RelRelaxation);
            }
            else
            {
               

                Debug.Log($"Calibration in progress: {math.GetCallibrationPercents()} %");
                Debug.Log($"Artifacts: {math.IsBothSidesArtifacted()}");

               
            }
        }
        catch (Exception ex)
        {
           Debug.Log(ex.ToString());
        }

    }

    private void EventBatteryChanged(ISensor sensor, int battPower)
    {

        kuk = battPower;
        //Debug.Log("Power: " + battPower);
    }
    private void EventSensorStateChanged(ISensor sensor, int battPower)
    {

        kuk = battPower;
        //Debug.Log("Power: " + battPower);
    }
    public void Progruz()
    {
        var mls = new MathLibSetting
        {
            sampling_rate = samplingFrequency,
            process_win_freq = 25,
            n_first_sec_skipped = 6,
            fft_window = samplingFrequency * 2,
            bipolar_mode = true,
            channels_number = 4,
            channel_for_analysis = 0
        };
        var ads = new ArtifactDetectSetting
        {
            art_bord = 110,
            allowed_percent_artpoints = 70,
            raw_betap_limit = 800_000,
            total_pow_border = 3 * 117,
            global_artwin_sec = 4,
            spect_art_by_totalp = false,
            num_wins_for_quality_avg = 100,
            hanning_win_spectrum = false,
            hamming_win_spectrum = true
        };
        var sads = new ShortArtifactDetectSetting
        {
            ampl_art_detect_win_size = 200,
            ampl_art_zerod_area = 200,
            ampl_art_extremum_border = 25
        };
        var mss = new MentalAndSpectralSetting
        {
            n_sec_for_averaging = 2,
            n_sec_for_instant_estimation = 2
        };
        math = new EegEmotionalMath(mls, ads, sads, mss);
        int calibrationLength = 8;
        math.SetCallibrationLength(calibrationLength);
        math.StartCalibration();
       
        // type of evaluation of instant mental levels
        bool independentMentalLevels = false;
        math.SetMentalEstimationMode(independentMentalLevels);

        // number of windows after the artifact with the previous actual value
        int nwinsSkipAfterArtifact = 10;
        math.SetSkipWinsAfterArtifact(nwinsSkipAfterArtifact);

        // calculation of mental levels relative to calibration values
        math.SetZeroSpectWaves(true, 0, 1, 1, 1, 0);

        // spectrum normalization by bandwidth
        math.SetSpectNormalizationByBandsWidth(true);

        

        
    }
   

}
