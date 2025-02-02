﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_ShopData : ScriptableObject
{
    public ShopPaymentTypes ShopPayment = ShopPaymentTypes.UnityIAP;
    public List<ShopVirtualCoins> CoinsPacks = new List<ShopVirtualCoins>();

    [Header("Settings")]
    public bool ShowFreeItems = true;

    public static string CompilePurchases(List<bl_ShopPurchase> purchases)
    {
        string line = "";
        for (int i = 0; i < purchases.Count; i++)
        {
            line += string.Format("{0},{1}-", purchases[i].TypeID, purchases[i].ID);
        }
        return line;
    }

    public static List<bl_ShopPurchase> DecompilePurchases(string line)
    {
        string[] split = line.Split("-"[0]);
        List<bl_ShopPurchase> list = new List<bl_ShopPurchase>();
        for (int i = 0; i < split.Length; i++)
        {
            if (string.IsNullOrEmpty(split[i])) continue;
            string[] info = split[i].Split(","[0]);

            bl_ShopPurchase sp = new bl_ShopPurchase();
            sp.TypeID = int.Parse(info[0]);
            sp.ID = int.Parse(info[1]);
            list.Add(sp);
        }
        return list;
    }

    private static bl_ShopData m_Data;
    public static bl_ShopData Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("ShopData", typeof(bl_ShopData)) as bl_ShopData;
            }
            return m_Data;
        }
    }

    [System.Serializable]
    public class ShopVirtualCoins
    {
        public string Name;
        public int Amount;
        public float Price;
    }

    [System.Serializable]
    public enum ShopPaymentTypes
    {
        Paypal = 0,
        UnityIAP = 1,
        Steam = 2,
        Other = 3,
    }
}