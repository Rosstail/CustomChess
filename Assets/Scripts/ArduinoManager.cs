using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using Newtonsoft.Json;
using System.IO;

public class ArduinoManager : MonoBehaviour
{
    [SerializeField] public string portName = "COM6";
    [SerializeField] public int portNumber = 9600;
    [SerializeField] public float caseLength = 4.0f; //Dist in cm
    [SerializeField] public float motorValuePerCase = 1000;
    SerialPort dataStream;
    private string receivedString;

    private void Awake()
    {
        dataStream = new SerialPort(portName, portNumber, Parity.None, 8, StopBits.One);
    }
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            dataStream.Open();
            dataStream.Write("OFF");
            dataStream.Write("RESET");
        } catch (IOException e)
        {
            Debug.LogError(e);
            Debug.Log("Datastream failed to open");
        }
        Debug.Log("is dataStream open ? " + dataStream.IsOpen);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void MoveArms(Vector2 origin, Vector2 target)
    {
        string originValue = origin.x + ";" + origin.y;
        string targetValue = (target.x - origin.x) + ";" + (target.y - origin.y);
        Debug.Log(targetValue);
        try
        {
            dataStream.Write(originValue);
            dataStream.Write("ON");
            dataStream.Write("0.5;0.5"); //Offset while holding piece
            dataStream.Write(targetValue);
            dataStream.Write("-0.5;-0.5"); //Offset while holding piece
            dataStream.Write("OFF");
            dataStream.Write("RESET");
        } catch
        {
            Debug.LogError("Problem with Datastream on move");
        }
    }
}
