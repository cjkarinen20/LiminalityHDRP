using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string RelativePath, T Data, bool Encrypted)
    {
        string path = Application.persistentDataPath + RelativePath;

        if (File.Exists(path))
        {
            try
            {
                Debug.Log("Data exists. Deleting old file and writing a new one.");
                File.Delete(path);
                using FileStream stream = File.Create(path);
                stream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(Data));
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save data: " + e.Message);
                return false;
            }
        }
        else
        {
            try
            {
                Debug.Log("Creating new file and writing data to it.");
                using FileStream stream = File.Create(path);
                stream.Close();
                File.WriteAllText(path, JsonConvert.SerializeObject(Data));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save data: " + e.Message);
                return false;
            }
        }
    }

    public T LoadData<T>(string RelativePath, bool Encrypted)
    {
        throw new System.NotImplementedException();
    }




}
