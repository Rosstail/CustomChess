using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class ArduinoManager : MonoBehaviour
{
    [SerializeField] public string portName = "COM7";
    [SerializeField] public int portNumber = 19200;
    [SerializeField] public float caseLength = 4000; //Dist in cm
    [SerializeField] public float motorValuePerCase = 16000;
    SerialPort dataStream;
    private string receivedString;

    private void Awake()
    {
        SerialPort dataStream = new SerialPort(portName, portNumber);
    }
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            dataStream.Open();
        } catch
        {
            Debug.Log("Datastream failed to open");
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            receivedString = dataStream.ReadLine();

            string[] data = receivedString.Split(',');

            foreach (string text in data)
            {
                Debug.Log(text);
            }
            Debug.Log("___");
        } catch
        {
            Debug.Log("Update.");
        }
    }

    public void MoveArms(Vector3 distances)
    {
        string value = distances.x + "," + distances.z;
        Debug.Log(value);
        try
        {
            dataStream.Write(value);
        } catch
        {
            Debug.LogError("Problem with Datastream on move");
        }
    }
}
