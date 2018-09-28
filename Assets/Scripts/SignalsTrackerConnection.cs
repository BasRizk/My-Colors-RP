using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class SignalsTrackerConnection{

	#region private members 	
	private DataController dataController;
	private TcpClient socketConnection; 
	private Thread clientReceiveThread; 
	private string ip_address = "localhost";
	private int port_num = 18000;	
	public int maxTries = 5;

	private string currentRecordFile;
	private bool recording;
	private List<string> singleRecordFileData;
	private string RECORDINGS_PATH;

	
	public SignalsTrackerConnection(DataController appDataController) {
		dataController = appDataController;
		RECORDINGS_PATH = dataController.EEG_LOGS_PATH;
		singleRecordFileData = new List<string>();
		recording = false;
	}
	

	//private float timeoutAfter;
	#endregion  	

	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	

	public void AbortThreads() {
		socketConnection.Close();
		clientReceiveThread.Abort();
		maxTries = 0;
	}
	public void ConnectToTcpServer () { 		
		try {  			
			Thread.Sleep(1000);
			socketConnection = new TcpClient();
			UnityEngine.Debug.Log("socketConnection TcpClient has been created.");
			
			socketConnection.Connect(ip_address, port_num);
			UnityEngine.Debug.Log("socketConnection TcpClient has connected.");
	
		} catch (Exception e) {
			UnityEngine.Debug.Log("On client connect exception " + e);
		} finally {
			if(isConnected()){
				UnityEngine.Debug.Log("Signals Tracker has been connected successfully.");
				SendMessage("Test Connection");
				clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
				clientReceiveThread.IsBackground = true; 			
				clientReceiveThread.Start();
			} else if(maxTries > 0) {
				maxTries--;
				UnityEngine.Debug.Log("Signals Tracker failed to connect with server..will try again.");
				ConnectToTcpServer();
				//dataController.HandleError();
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
						if (serverMessage.Split(',').Length < 5) {
							Debug.Log("server message received as: " + serverMessage); 
						} else {
							if(recording) {
								singleRecordFileData.Add(serverMessage);
								Debug.Log("Data received = " + serverMessage);
							} else {
								if(singleRecordFileData != null && singleRecordFileData.Count != 0) {
									CreateRecordFile();
									singleRecordFileData.Clear();
									Debug.Log("File should be created");

								}
							}
						}						
					} 				
				} 			
			}
		}         
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}     
	} 

	private void CreateRecordFile() {
		Debug.Log("Creating new record file"); 
		var csv = new StringBuilder();
		var newLine = ""; 
		foreach (string record in singleRecordFileData) {
			newLine = record;
			csv.AppendLine(newLine);  
		}

		string records_file_path = Path.Combine(RECORDINGS_PATH, currentRecordFile + ".csv");
		File.AppendAllText(records_file_path, csv.ToString());
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
		//currentRecordFile = filename;
		//recording = true;
	}
	public void StopRecord() {
		//byte[] data = Encoding.ASCII.GetBytes("CyKITv2:::RecordStop");
		SendMessage("CyKITv2:::RecordStop");
		//recording = false;
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
