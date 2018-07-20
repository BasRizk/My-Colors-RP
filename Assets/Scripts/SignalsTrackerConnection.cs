using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

public class SignalsTrackerConnection{

	#region private members 	
	private DataController dataController;
	private TcpClient socketConnection; 
	private Thread clientReceiveThread; 
	private string ip_address = "localhost";
	private int port_num = 18000;	

	public SignalsTrackerConnection(DataController appDataController) {
		dataController = appDataController;
	}
	//
	//private float timeoutAfter;
	#endregion  	

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	public void ConnectToTcpServer () { 		
		try {  			
			socketConnection = new TcpClient();
			UnityEngine.Debug.Log("socketConnection TcpClient has been created.");
			
			socketConnection.Connect(ip_address, port_num);
			UnityEngine.Debug.Log("socketConnection TcpClient has connected.");
			SendMessage("Test Connection");
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();
	
		} catch (Exception e) { 			
			UnityEngine.Debug.Log("On client connect exception " + e);
		} finally {
			if(isConnected()){
			UnityEngine.Debug.Log("Signals Tracker has been connected successfully.");
			} else {
				UnityEngine.Debug.Log("Signals Tracker failed to connect with server.");
				dataController.HandleError();
			}
		}

		
	
	} 

	/// <summary> 	
	/// Runs in background clientReceiveThread; Listens for incomming data. 	
	/// </summary>     

	private void ListenForData() { 		
		try { 			

			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using ( NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 
						if (serverMessage.Split(',').Length < 5)				
							Debug.Log("server message received as: " + serverMessage); 							
					} 				
				} 			
			}
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	}  	

	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	private void SendMessage(string clientMessage) {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {    
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Message sent: " + clientMessage + " - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 

	public void StartRecord(string filename) {
    	//byte[] data = Encoding.ASCII.GetBytes("CyKITv2:::RecordStart:::"+ filename);
		//stream.Write(data, 0, data.Length);
		SendMessage("CyKITv2:::RecordStart:::"+ filename);

	}
	public void StopRecord() {
		//byte[] data = Encoding.ASCII.GetBytes("CyKITv2:::RecordStop");
		SendMessage("CyKITv2:::RecordStop");
	}
	public void closeConnection(){
		socketConnection.Close();
		UnityEngine.Debug.Log("Signals Tracker has been disconnected.");
	}

	public bool isConnected() {
		if(socketConnection != null) {
			return socketConnection.Connected;
		}
		return false;
	}
}
