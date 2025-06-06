using System.Collections;
using System.Collections.Generic;
using SpatialLLM.Device;
using SpatialLLM.Experiment;
using UnityEngine;
using UnityEngine.iOS;

public class ArrangementDebugger : MonoBehaviour
{
    private DeviceArrangementGenerator arrangementGenerator;


    public int currentArrangementIndex = 0;

    [SerializeField] private string arrangeId = "";




    private void Start()
    {
        this.arrangementGenerator = FindObjectOfType<DeviceArrangementGenerator>();
    }



    public void TurnOnPreviousArrangement()
    {
        currentArrangementIndex = (currentArrangementIndex - 1 + arrangementGenerator.GetDeviceArrangeDatas().Count) % arrangementGenerator.GetDeviceArrangeDatas().Count;
        TurnOn();
    }

    public void TurnOnNextArrangement()
    {
        currentArrangementIndex = (currentArrangementIndex + 1) % arrangementGenerator.GetDeviceArrangeDatas().Count;
        TurnOn();
    }

    public void TurnOn()
    {
        DeviceArrangeData currentArrangement = arrangementGenerator.GetDeviceArrangeDatas()[currentArrangementIndex];

        foreach (SADevice saDevice in SADeviceRef.Instance.GetAllDevices())
        {
            saDevice.TurnOff();
        }

        foreach (DeviceColorPair device in currentArrangement.devices)
        {
            // Assuming DeviceManager has a method to turn on devices by color
            SADevice saDevice = device.device as SADevice;
            DrawOnHover drawOnHover = saDevice.GetComponent<DrawOnHover>();
            saDevice.TurnOnWithColor(device.GetUnityColor());
            if (drawOnHover != null)
            {
                drawOnHover.VisualizeTargetDevice(Color.blue);
            }
        }
    }
    

    public void TurnOnWithId()
    {
        if (string.IsNullOrEmpty(arrangeId))
        {
            Debug.LogWarning("Arrange ID is not set.");
            return;
        }

        DeviceArrangeData currentArrangement = arrangementGenerator.GetDeviceArrangeDatas().Find(arr => arr.device_arrange_id == arrangeId);

        if (currentArrangement == null)
        {
            Debug.LogWarning($"No arrangement found with ID: {arrangeId}");
            return;
        }

        foreach (SADevice saDevice in SADeviceRef.Instance.GetAllDevices())
        {
            saDevice.TurnOff();
        }

        foreach (DeviceColorPair device in currentArrangement.devices)
        {
            SADevice saDevice = device.device as SADevice;
            DrawOnHover drawOnHover = saDevice.GetComponent<DrawOnHover>();
            saDevice.TurnOnWithColor(device.GetUnityColor());
            if (drawOnHover != null)
            {
                drawOnHover.VisualizeTargetDevice(Color.blue);
            }
        }
    }



}
