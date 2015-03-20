﻿// Generated by .NET Reflector from C:\Projects\Skylines\DifficultyMod\DifficultyMod\libs\Assembly-CSharp.dll
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace DifficultyMod
{
    public class WBCommercialBuildingAI : CommercialBuildingAI
    {
        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            if (buildingData.m_fireIntensity != 0 && frameData.m_fireDamage > 12)
            {
                WBBResidentialBuildingAI.ExtraFireSpread(buildingID, ref buildingData, 50, this.m_info.m_size.y);
            }
        }
    }

}

