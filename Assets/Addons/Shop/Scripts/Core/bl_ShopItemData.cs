﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if PSELECTOR
using MFPS.PlayerSelector;
#endif

[Serializable]
public class bl_ShopItemData 
{
    public string Name;
    public int Price;
    public ShopItemType Type = ShopItemType.Weapon;

    public int ID;
    public bl_GunInfo GunInfo;
#if PSELECTOR
    public bl_PlayerSelectorInfo PlayerSkinInfo;
#endif

}