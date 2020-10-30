﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class FileManager : MonoBehaviour {
    public PlayerData playerData;
    private string playerDataLocalPath = "/PlayerData.dat";
    private string playerDataLocalPathBackup = "/PlayerDataBackup.dat";
    public FloatValue autoSaveInSeconds;

    public TextMeshProUGUI textTest;
    private void Start() {
        Load();
        StartCoroutine(AutomaticBackupSave());
    }

    private IEnumerator AutomaticBackupSave() {
        yield return new WaitForSeconds(autoSaveInSeconds.value);
        Save(playerDataLocalPathBackup);
        StartCoroutine(AutomaticBackupSave());
    }

    public void Save(string localPath) {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(Application.persistentDataPath + localPath, FileMode.Create);
        playerData.OnBeforeSerialize();
        var json = JsonUtility.ToJson(playerData);
        binaryFormatter.Serialize(fileStream, json);
        fileStream.Close();
    }

    public void Load() {
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
        JsonUtility.FromJsonOverwrite(json, playerData);
        fileStream.Close();
        playerData.OnAfterDeserialize();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if(pauseStatus)
            Save(playerDataLocalPath);
    }
}