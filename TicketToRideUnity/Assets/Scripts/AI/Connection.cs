using System;
using System.Net.Sockets;
using System.IO;
using Assets.Scripts.Utility;
using System.Threading;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.AI
{
    public class Connection
    {
         // TCP-Client
        private TcpClient mClient;

        // Data Stream.
        private Stream mStream;

        // This method establishes the connection
        private void InitiateConnection()
        {
            mClient = new TcpClient();
            mClient.Connect(Constants.HOST_ADDRESS, Constants.PORT);
            mStream = mClient.GetStream();
        }

        ~Connection()
        {
            CloseConnection();
        }

        // This method closes the connection.
        public void CloseConnection()
        {
            if (mClient != null && mClient.Connected)
            {
                mClient.Close();
            }
        }

        // Sending a request to the server     
        public string SendRequest(object request)
        {
            InitiateConnection();

		    string json = JsonUtility.ToJson(request) + Environment.NewLine;
            UnityEngine.Debug.Log("Request is: " + json);
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] data = asen.GetBytes(json);

            // Send request
            mStream.Write(data, 0, data.Length);

            // Let the thread sleep to make sure the server answered the request
            Thread.Sleep(100);

            // Reading the answer
            byte[] responseData = new byte[16384];
            string textReceived = "";
            int read = 0;
            do
            {
                read = mStream.Read(responseData, 0, responseData.Length);
                for (int i = 0; i < read; i++)
                {
                    textReceived += (char)responseData[i];
                }
            } while (read > 0);

            //UnityEngine.Debug.Log("Text received: " + textReceived);

            // the received answer
            string answer = textReceived.ToString();

            CloseConnection();

            return answer;
        }

    }

}

