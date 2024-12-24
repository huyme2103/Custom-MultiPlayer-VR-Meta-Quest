using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class ConnectUIScript : MonoBehaviour
{
    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button clientButton;

    private const int broadcastPort = 8888;
    private UdpClient udpClient;
    private bool isListening = false;

    private string discoveredIP = "";

    private void Start()
    {
        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    private void HostButtonOnClick()
    {
        // Bắt đầu host và phát sóng địa chỉ IP
        NetworkManager.Singleton.StartHost();
        StartCoroutine(BroadcastServerIP());
    }

    private void ClientButtonOnClick()
    {
        StartCoroutine(ListenForServerIP());
    }

    private IEnumerator BroadcastServerIP()
    {
        using (UdpClient udpServer = new UdpClient())
        {
            udpServer.EnableBroadcast = true;

            while (true)
            {
                try
                {
                    string localIP = GetLocalIPAddress();
                    string message = "ServerHere:" + localIP;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udpServer.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, broadcastPort));
                    Debug.Log("Broadcasting IP: " + localIP);
                }
                catch (SocketException ex)
                {
                    Debug.LogError("Broadcast error: " + ex.Message);
                }

                yield return new WaitForSeconds(1f); // Gửi gói tin mỗi giây
            }
        }
    }

    private IEnumerator ListenForServerIP()
    {
        isListening = true;

        using (udpClient = new UdpClient(broadcastPort))
        {
            udpClient.EnableBroadcast = true;

            while (isListening && string.IsNullOrEmpty(discoveredIP))
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, broadcastPort);

                try
                {
                    // Nhận gói tin UDP
                    if (udpClient.Available > 0)
                    {
                        byte[] data = udpClient.Receive(ref remoteEP);
                        string message = Encoding.UTF8.GetString(data);

                        if (message.StartsWith("ServerHere"))
                        {
                            discoveredIP = message.Split(':')[1];
                            Debug.Log("Discovered server IP: " + discoveredIP);
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Debug.LogError("Listening error: " + ex.Message);
                }

                yield return null;
            }
        }

        if (!string.IsNullOrEmpty(discoveredIP))
        {
            ConnectToServer(discoveredIP);
        }
        else
        {
            Debug.LogError("Failed to discover server IP.");
        }
    }

    private void ConnectToServer(string serverIP)
    {
        Debug.Log("Connecting to server: " + serverIP);

        // Gán địa chỉ IP cho UnityTransport
        NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().ConnectionData.Address = serverIP;

        // Bắt đầu Client
        NetworkManager.Singleton.StartClient();
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void OnDestroy()
    {
        isListening = false;
        udpClient?.Close();
    }
}
