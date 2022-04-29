using System;
using UnityEngine;
using FGear;

public class CarManager : MonoBehaviour
{
    [Serializable]
    class PlayerVehicle
    {
        public string PlayerName = "Player";

        public Vehicle Vehicle;

        [NonSerialized]
        public float Progress = 0f;

        [NonSerialized]
        public float LastProgress = 0f;
    }

    [SerializeField]
    bool ShowGUI = true;

    [SerializeField]
    bool ShowRankings = false;

    [SerializeField]
    PlayerVehicle[] VehicleList;

    [SerializeField]
    int StartIndex = 0;

    [SerializeField]
    bool MuteOthers = false;

    [SerializeField]
    OrbitCamera Camera;

    [SerializeField]
    SplineTool Spline;

    [SerializeField]
    float CountDown = 0f;

    Rect mWindowRect = new Rect(Screen.width - 260, 5, 125, 115);

    int mWinID;

    int mCurrentIndex = 0;

    float mFreezeTime = 0f;

    float mSplineMaxParam = 0f;

    void Start()
    {
        mWinID = Utility.winIDs++;

        mCurrentIndex = StartIndex;
        if (mCurrentIndex >= VehicleList.Length) mCurrentIndex = 0;

        //activate current
        setCurrentActive();

        //count down at start?
        mFreezeTime = CountDown;
        if (mFreezeTime > 0f) setAllFrozen(true);

        //if spline exists, calc. initial vehicle progress/rankings
        if (VehicleList.Length > 0 && Spline != null)
        {
            mSplineMaxParam = Spline.getPath().GetMaxParam();

            for (int i = 0; i < VehicleList.Length; i++)
            {
                float currentPrm = Spline.getClosestParam(VehicleList[i].Vehicle.getPosition());
                VehicleList[i].Progress = currentPrm;
                VehicleList[i].LastProgress = currentPrm;
            }
        }
    }

    void Update()
    {
        //still frozen?
        if (mFreezeTime > 0f)
        {
            mFreezeTime -= Time.deltaTime;
            if (mFreezeTime <= 0f)
            {
                setAllFrozen(false);
            }
        }
        else if (ShowRankings) updateProgress();

        //reset vehicle with R key
        if (mCurrentIndex < VehicleList.Length)
        {
            Vehicle v = VehicleList[mCurrentIndex].Vehicle;

            if (Input.GetKeyDown(KeyCode.R))
            {
                Vector3 pos = v.getPosition();
                Quaternion rot = v.getRotation();
                rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);

                //if spline found, set pos/rot acc. to current param
                if (Spline != null)
                {
                    float param = Spline.getClosestParam(pos);
                    pos = Spline.getPoint(param);
                    Vector3 target = Spline.getPoint(param + 0.1f);
                    rot.SetLookRotation(target - pos);
                }

                v.reset(pos, rot);
            }
        }
    }

    void setVehicleActive(int index, bool active)
    {
        //activate/deactivate components acc. to active state
        Vehicle v = VehicleList[index].Vehicle;
        AIController ai = v.GetComponent<AIController>();
        bool isAI = ai != null && ai.enabled && ai.isActive();
        v.getStandardInput().setEnabled(active && !isAI);
        v.getStandardInput().resetInputs();
        if (v.GetComponent<Statistics>() != null) v.GetComponent<Statistics>().enabled = active;
        if (v.GetComponent<GaugeUI>() != null) v.GetComponent<GaugeUI>().enabled = active;
        if (v.GetComponent<MyDebug>() != null) v.GetComponent<MyDebug>().enabled = active;
        if (v.GetComponent<Effects>() != null) v.GetComponent<Effects>().setVolume((active || !MuteOthers) ? 1f : 0f);

        //focus camera on active vehicle
        if (active) Camera.setTarget(v.transform);

        //active vehicles minimap quad has different color
        GameObject miniQuad = Utility.findChild(v.gameObject, "minimapQuad");
        if (miniQuad != null)
        {
            miniQuad.GetComponent<Renderer>().material.color = active ? Color.red : Color.blue;
            miniQuad.transform.localPosition = (active ? 20f : 10f) * Vector3.up;
        }
    }

    void setCurrentActive()
    {
        for (int i = 0; i < VehicleList.Length; i++)
        {
            setVehicleActive(i, i == mCurrentIndex);
        }
    }

    //called at start grid
    void setAllFrozen(bool freeze)
    {
        for (int i = 0; i < VehicleList.Length; i++)
        {
            Vehicle v = VehicleList[i].Vehicle;
            v.getStandardInput().setStartGridMode(freeze);
            v.setBraking(1f, false);
            AIController ai = v.GetComponent<AIController>();
            if (ai != null) ai.enabled = !freeze;
        }
    }

    void updateProgress()
    {
        if (Spline != null)
        {
            //update vehicle progress on spline
            for (int i = 0; i < VehicleList.Length; i++)
            {
                Vehicle v = VehicleList[i].Vehicle;
                AIController ai = v.GetComponent<AIController>();

                float currentPrm = 0f;
                //for ai progress is already found, read from ai component
                if (ai != null && ai.isActive())
                {
                    currentPrm = ai.getCurrentSplineParam();
                    if (currentPrm < 0f) continue;
                }
                //for user controlled vehice, find current param
                else currentPrm = Spline.getClosestParam(VehicleList[i].Vehicle.getPosition());

                //how much change from last frame
                float lastPrm = VehicleList[i].LastProgress;
                VehicleList[i].LastProgress = currentPrm;
                float progress = currentPrm - lastPrm;

                //check pass from finish line case
                if (progress < -0.5f * mSplineMaxParam) progress = mSplineMaxParam + progress;

                //add
                VehicleList[i].Progress += progress;
            }

            //vehicle list should be sorted according to vehicle progress/ranking
            Vehicle cVehicle = VehicleList[mCurrentIndex].Vehicle;
            Array.Sort(VehicleList,
                delegate(PlayerVehicle p1, PlayerVehicle p2) { return p2.Progress.CompareTo(p1.Progress); });

            //refind correct index of current vehicle
            for (int i = 0; i < VehicleList.Length; i++)
            {
                if (cVehicle == VehicleList[i].Vehicle)
                {
                    mCurrentIndex = i;
                    break;
                }
            }
        }
    }

    void OnGUI()
    {
        if (mFreezeTime > 0f)
        {
            GUI.skin.label.fontSize = 144;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            float x = 0.5f * Screen.width;
            GUI.Label(new Rect(x - 175, 0f, 350, 200f), mFreezeTime > 0.5f ? mFreezeTime.ToString("f0") : "GO!");
            GUI.skin.label.fontSize = 12;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }
        else
        {
            if (ShowGUI)
            {
                mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Car Select");
            }

            if (ShowRankings)
            {
                GUI.skin.label.fontSize = 15;
                for (int i = 0; i < VehicleList.Length; i++)
                {
                    GUI.contentColor = i == mCurrentIndex ? Color.red : Color.white;
                    GUI.Label(new Rect(Screen.width - 125f, 20f + i * 30f, 200f, 30f),
                        (i + 1).ToString().PadRight(3) + VehicleList[i].PlayerName);
                }

                GUI.skin.label.fontSize = 12;
                GUI.contentColor = Color.white;
            }
        }
    }

    void uiWindowFunction(int windowID)
    {
        if (GUI.Button(new Rect(10, 25, 105, 25), "Next Car"))
        {
            mCurrentIndex++;
            mCurrentIndex %= VehicleList.Length;
            setCurrentActive();
        }

        if (GUI.Button(new Rect(10, 55, 105, 25), "Toggle Engine"))
        {
            bool running = VehicleList[mCurrentIndex].Vehicle.getEngine().isRunning();
            VehicleList[mCurrentIndex].Vehicle.getEngine().setEngineRunning(!running);
        }

        AIController ai = VehicleList[mCurrentIndex].Vehicle.GetComponent<AIController>();
        bool isAI = ai != null && ai.enabled && ai.isActive();
        isAI = GUI.Toggle(new Rect(10, 85, 105, 20), isAI, isAI ? " AI Enabled" : "AI Disabled");
        VehicleList[mCurrentIndex].Vehicle.getStandardInput().setEnabled(!isAI);
        if (ai != null) ai.setActive(isAI);
    }
}