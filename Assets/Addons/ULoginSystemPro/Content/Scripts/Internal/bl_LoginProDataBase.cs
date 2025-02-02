﻿using UnityEngine;
using System;

public class bl_LoginProDataBase : ScriptableObject
{

    [Header("Host Path")]
    [Tooltip("The Url of folder where your php scripts are located in your host.")]
    public string PhpHostPath;
    public string SecretKey = "123456";
    public string OnLoginLoadLevel = "NextLevelName";

    [Header("Settings")]
    public bool CheckGameVersion = true;
    public bool AutomaticallyLoadScene = false;
    public bool UpdateIP = true;
    public bool DetectBan = true;
    public bool RequiredEmailVerification = true;
    public bool CanRegisterSameEmail = false;
    public bool BanComprobationInMid = true; //keep check ban each certain time
    public bool PlayerCanChangeNick = true; // can players change their nick name?
    public bool ForceLoginScene = true;
    [Range(3, 12)] public int MinPasswordLenght = 5;
    [Range(10, 300)] public int CheckBanEach = 10;
    [Tooltip("Check that the user name doesn't contain a bad word from the black word list.")]
    public bool FilterUserNames = true;
#if CLANS
    public int CreateClanPrice = 1500;
    public bool DeleteEmptyClans = true;
#endif

    [Header("Script Names")]
    public string LoginPhp = "bl_Login";
    public string RegisterPhp = "bl_Register";
    public string DataBasePhp = "bl_DataBase";
    public string RankingPhp = "bl_Ranking";
    public string InitPhp = "bl_Init";
    public string BanListPhp = "bl_BanList";
    public string BanPhp = "bl_Ban";
    public string ResetPassword = "bl_ResetPassword";
    public string ChangePassword = "bl_ChangePassword";
    public string RequestUser = "bl_RequestUser";
    public string SupportPhp = "bl_Support";
    public string DataBaseCreator = "bl_DatabaseCreator";
    public string ClanPhp = "bl_Clan";
    public string ShopPhp = "bl_Shop";

#if UNITY_EDITOR
    [Header("Editor Only")]
    public string HostName;
    public string DataBaseName;
    public string DataBaseUser;
    public string Passworld;
#endif

    public readonly string[] UserNameFilters = new string[] { "fuck", "fucker", "motherfucker", "nigga", "nigger", "porn", "pussy", "cock", "anus", "racist", "vih", "puto", "fagot", "shit", "bullshit", "gay",
     "sex", "nazi",};

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    public string GetUrl(URLType _type)
    {
        string scriptName = "None";
        switch (_type)
        {
            case URLType.Login:
                scriptName = LoginPhp;
                break;
            case URLType.Register:
                scriptName = RegisterPhp;
                break;
            case URLType.DataBase:
                scriptName = DataBasePhp;
                break;
            case URLType.Init:
                scriptName = InitPhp;
                break;
            case URLType.BanList:
                scriptName = BanListPhp;
                break;
            case URLType.Ranking:
                scriptName = RankingPhp;
                break;
            case URLType.Ban:
                scriptName = BanPhp;
                break;
            case URLType.ResetPassword:
                scriptName = ResetPassword;
                break;
            case URLType.ChangePassword:
                scriptName = ChangePassword;
                break;
            case URLType.RequestUser:
                scriptName = RequestUser;
                break;
            case URLType.Support:
                scriptName = SupportPhp;
                break;
            case URLType.Creator:
                scriptName = DataBaseCreator;
                break;
            case URLType.Clans:
                scriptName = ClanPhp;
                break;
            case URLType.Shop:
                scriptName = ShopPhp;
                break;
        }
        string url = string.Format("{0}{1}.php", PhpHostPath, scriptName);
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { Debug.Log("URL is not well formed, please check if your php script have the same name and have assign the host path."); }
        return url;
    }


    private static bl_LoginProDataBase _dataBase;
    public static bl_LoginProDataBase Instance
    {
        get
        {
            if (_dataBase == null) { _dataBase = Resources.Load("LoginDataBasePro", typeof(bl_LoginProDataBase)) as bl_LoginProDataBase; }
            return _dataBase;
        }
    }

    public int CanRegisterSameEmailInt()
    {
        return (CanRegisterSameEmail == true) ? 1 : 0;
    }

    public int RequiereVerification()
    {
        return (RequiredEmailVerification == true) ? 0 : 1;
    }

    public bool FilterName(string userName)
    {
        userName = userName.ToLower();
        for (int i = 0; i < UserNameFilters.Length; i++)
        {
            if (userName.Contains(UserNameFilters[i].ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    [Serializable]
    public enum URLType
    {
        Login,
        Register,
        DataBase,
        Ranking,
        Init,
        BanList,
        Ban,
        ResetPassword,
        ChangePassword,
        RequestUser,
        Support,
        Creator,
        Clans,
        Shop,
    }
}