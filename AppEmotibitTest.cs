using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class AppEmotibitTest /* : MonoBehaviour */
{

    //public Text txtRecording;
    //public Text txtMessages;
    //public Text txtData;

    public int maxMessages = 7;


    private Queue<string> messageQueue = new Queue<string>();
    private Queue<string> dataQueue = new Queue<string>();


    void Start()
    {
        Emotibit.instance.onBatteryLevelLow = () =>
        {
            LogMessage(DateTime.Now.ToString("HH:mm:ss - ") + "Emotibit - low battery!!!");
        };

        Emotibit.instance.onDataTimeoutReceived = () =>
        {
            LogMessage(DateTime.Now.ToString("HH:mm:ss - ") + "Emotibit - timeout data!!!");
        };


        Emotibit.instance.onBiometricDataReceived = (x) =>
        {
            LogData(DateTime.Now.ToString("HH:mm:ss - ") + x);
        };
        
    }

    
    void Update()
    {

        /*
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Emotibit.instance.Play();
            txtRecording.text = "Recording True";
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Emotibit.instance.Stop("/log");
            LogMessage("Saved data on: \n" + Directory.GetCurrentDirectory() + "/log");
            txtRecording.text = "Recording False";
        }
        */

    }

    public void LogMessage(string message)
    {
        if (messageQueue.Count >= maxMessages)
        {
            messageQueue.Dequeue(); 
        }

        messageQueue.Enqueue(message); 
        //txtMessages.text = string.Join("\n", messageQueue.ToArray());
    }

    public void LogData(string message)
    {
        if (dataQueue.Count >= maxMessages)
        {
            dataQueue.Dequeue();
        }

        dataQueue.Enqueue(message);
        //txtData.text = string.Join("\n", dataQueue.ToArray());
    }
}
