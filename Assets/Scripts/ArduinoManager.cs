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
        } catch (IOException e)
        {
            Debug.LogError(e);
            Debug.Log("Datastream failed to open");
        } finally
        {
            Debug.Log("is dataStream open ? " + dataStream.IsOpen);
            if (dataStream.IsOpen)
            {
                string[] orders = { "OFF", "RESET" };
                StartCoroutine(Waiter(1, orders));
            }
        }
    }

    IEnumerator Waiter(float waitTime, string[] orders)
    {
        foreach (string order in orders) {
            Debug.Log("sent order " + order);
            dataStream.Write(order);
            yield return new WaitForSecondsRealtime(waitTime);
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void MoveArms(Vector2 origin, Vector2 target)
    {
        string originValue = origin.x + ";" + origin.y;
        string targetValue = (target.x - origin.x) + ";" + (target.y - origin.y);
        try
        {
            string[] orders = {
                originValue,
                "ON",
                "0.5;0.5",
                targetValue,
                "-0.5;-0.5",
                "OFF",
                "RESET"
            };
            StartCoroutine(Waiter(1, orders));
        } catch
        {
            Debug.LogError("Problem with Datastream on move");
        }
    }
}
