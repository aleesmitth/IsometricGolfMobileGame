﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class FileManager : MonoBehaviour {
    public PlayerDataBuffer playerDataBuffer;
    public PlayerData playerData;
    private const string playerDataLocalPath = "/PlayerData.dat";
    private const string playerDataLocalPathBackup = "/PlayerDataBackup.dat";
    public FloatValue autoSaveInSeconds;
    
    private void Start() {
        Load();
        StartCoroutine(AutomaticBackupSave());
    }

    private IEnumerator AutomaticBackupSave() {
        yield return new WaitForSeconds(autoSaveInSeconds.value);
        Save(playerDataLocalPathBackup);
        StartCoroutine(AutomaticBackupSave());
    }

    public void Save(string localPath = playerDataLocalPath) {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(Application.persistentDataPath + localPath, FileMode.Create);
        playerDataBuffer.OnBeforeSerialize(playerData);
        var json = JsonUtility.ToJson(playerDataBuffer);
        binaryFormatter.Serialize(fileStream, json);
        fileStream.Close();
    }

    private void Load() {
        string validLocalPath;
        if (File.Exists(Application.persistentDataPath + playerDataLocalPath)) {
            validLocalPath = "/PlayerData.dat";
        }
        else if (File.Exists(Application.persistentDataPath + playerDataLocalPathBackup)) {
            validLocalPath = "/PlayerDataBackup.dat";
        }
        else return;
        
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(Application.persistentDataPath + validLocalPath, FileMode.Open);
        var json = binaryFormatter.Deserialize(fileStream) as string;
        
        //var deserializedData = JsonUtility.FromJson<PlayerDataBuffer>(json); not supported for scriptable objects
        fileStream.Close();
        JsonUtility.FromJsonOverwrite(json, playerDataBuffer);
        playerDataBuffer.OnAfterDeserialize(playerData);
    }

    private void OnApplicationPause(bool pauseStatus) {
        if(pauseStatus)
            Save(playerDataLocalPath);
    }
}
