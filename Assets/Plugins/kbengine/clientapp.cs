using UnityEngine;
using System;
using System.IO;  
using System.Collections;
using KBEngine;

public class clientapp : MonoBehaviour {
	public static KBEngineApp gameapp = null;
	
	void Awake() 
	 {
		DontDestroyOnLoad(transform.gameObject);
	 }
 
	// Use this for initialization
	void Start () 
	{
		MonoBehaviour.print("clientapp::start()");
			
		KBEngine.Event.registerOut("onImportClientMessages", this, "onImportClientMessages");
		KBEngine.Event.registerOut("onImportServerErrorsDescr", this, "onImportServerErrorsDescr");
		KBEngine.Event.registerOut("onImportClientEntityDef", this, "onImportClientEntityDef");
		KBEngine.Event.registerOut("onVersionNotMatch", this, "onVersionNotMatch");
		KBEngine.Event.registerOut("onScriptVersionNotMatch", this, "onScriptVersionNotMatch");
		KBEngine.Event.registerOut("onServerDigest", this, "onServerDigest");
		
		gameapp = new KBEngineApp();
		KBEngineApp.url = "http://127.0.0.1";
		KBEngineApp.app.clientType = 5;
		KBEngineApp.app.ip = "127.0.0.1";
		KBEngineApp.app.port = 20013;
		
		//gameapp.autoImportMessagesFromServer(true);
		
		byte[] loginapp_onImportClientMessages = loadFile (Application.persistentDataPath, "loginapp_clientMessages." + 
		                                                   KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);

		byte[] baseapp_onImportClientMessages = loadFile (Application.persistentDataPath, "baseapp_clientMessages." + 
		                                                  KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);

		byte[] onImportServerErrorsDescr = loadFile (Application.persistentDataPath, "serverErrorsDescr." + 
		                                             KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);

		byte[] onImportClientEntityDef = loadFile (Application.persistentDataPath, "clientEntityDef." + 
		                                           KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);

		if(loginapp_onImportClientMessages.Length > 0 && baseapp_onImportClientMessages.Length > 0)
		{
			KBEngineApp.app.importMessagesFromMemoryStream (loginapp_onImportClientMessages, baseapp_onImportClientMessages, onImportClientEntityDef, onImportServerErrorsDescr);
		}
	}
	
	void OnDestroy()
	{
		MonoBehaviour.print("clientapp::OnDestroy(): begin");
		KBEngineApp.app.destroy();
		MonoBehaviour.print("clientapp::OnDestroy(): over, isbreak=" + gameapp.isbreak + ", over=" + gameapp.kbethread.over);
	}
	
	void FixedUpdate () {
		KBEUpdate();
	}
		
	void KBEUpdate()
	{
		KBEngine.Event.processOutEvents();
	}

	public void onImportClientMessages(string currserver, byte[] stream)
	{
		if(currserver == "loginapp")
			createFile (Application.persistentDataPath, "loginapp_clientMessages." + 
			            KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion, stream);
		else
			createFile (Application.persistentDataPath, "baseapp_clientMessages." + 
			            KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion, stream);
	}

	public void onImportServerErrorsDescr(byte[] stream)
	{
		createFile (Application.persistentDataPath, "serverErrorsDescr." + 
		            KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion, stream);
	}
	
	public void onImportClientEntityDef(byte[] stream)
	{
		createFile (Application.persistentDataPath, "clientEntityDef." + 
		            KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion, stream);
	}
	
	public void clearMessageFiles()
	{
		deleteFile(Application.persistentDataPath, "loginapp_clientMessages." + KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);
		deleteFile(Application.persistentDataPath, "baseapp_clientMessages." + KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);
		deleteFile(Application.persistentDataPath, "serverErrorsDescr." + KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);
		deleteFile(Application.persistentDataPath, "clientEntityDef." + KBEngineApp.app.clientVersion + "." + KBEngineApp.app.clientScriptVersion);
		KBEngineApp.app.resetMessages();
	}
	
	public void onVersionNotMatch(string verInfo, string serVerInfo)
	{
		clearMessageFiles();
	}

	public void onScriptVersionNotMatch(string verInfo, string serVerInfo)
	{
		clearMessageFiles();
	}
	
	public void onServerDigest(string currserver, string serverProtocolMD5, string serverEntitydefMD5)
	{
		// 我们不需要检查网关的协议， 因为登录loginapp时如果协议有问题已经删除了旧的协议
		if(currserver == "baseapp")
			return;
		
		if(loadFile(Application.persistentDataPath, serverProtocolMD5 + serverEntitydefMD5).Length == 0)
		{
			Debug.LogError("onServerDigest: not found (" + serverProtocolMD5 + serverEntitydefMD5 + ")");
			clearMessageFiles();

			createFile(Application.persistentDataPath, serverProtocolMD5 + serverEntitydefMD5, new byte[1]{0});
		}
	}
	
	void createFile(string path, string name, byte[] datas)  
	{  
		deleteFile(path, name);
		Debug.Log("createFile: " + path + "//" + name);
		FileStream fs = new FileStream (path + "//" + name, FileMode.OpenOrCreate, FileAccess.Write);
		fs.Write (datas, 0, datas.Length);
		fs.Close ();
		fs.Dispose ();
	}  
   
   byte[] loadFile(string path, string name)  
   {  
		FileStream fs;

		try{
			fs = new FileStream (path + "//" + name, FileMode.Open, FileAccess.Read);
		}
		catch (Exception e)
		{
			Debug.Log("loadFile: " + path + "//" + name);
			Debug.Log(e.ToString());
			return new byte[0];
		}

		byte[] datas = new byte[fs.Length];
		fs.Read (datas, 0, datas.Length);
		fs.Close ();
		fs.Dispose ();

		Debug.Log("loadFile: " + path + "//" + name + ", datasize=" + datas.Length);
		return datas;
   }  
   
   void deleteFile(string path, string name)  
   {  
		Debug.Log("deleteFile: " + path + "//" + name);
		try{
        	File.Delete(path + "//"+ name);  
		}
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
		}
   }  
}
