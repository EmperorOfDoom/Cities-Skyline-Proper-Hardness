﻿// Generated by .NET Reflector from C:\Projects\Skylines\DifficultyMod\DifficultyMod\libs\Assembly-CSharp.dll
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DifficultyMod
{
    public class WBBResidentialBuildingAI2 : ResidentialBuildingAI
    {
        private FireSpread fs = new FireSpread();

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);

            if (SaveData2.saveData.DifficultyLevel != DifficultyLevel.Vanilla)
            {
                Notification.Problem problem = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.TooFewServices);
                if (buildingData.m_customBuffer1 != 0 && buildingData.m_customBuffer1 < 10)
                {
                    buildingData.m_outgoingProblemTimer = (byte)Mathf.Min(0xff, buildingData.m_outgoingProblemTimer + 1);
                    if (buildingData.m_outgoingProblemTimer >= 200)
                    {
                        problem = Notification.AddProblems(problem, Notification.Problem.MajorProblem | Notification.Problem.TooFewServices);
                    }
                    else if (buildingData.m_outgoingProblemTimer >= 60)
                    {
                        problem = Notification.AddProblems(problem, Notification.Problem.TooFewServices);
                    }
                }
                else
                {
                    buildingData.m_outgoingProblemTimer = 0;
                }
                buildingData.m_problems = problem;
            }

            if (buildingData.m_fireIntensity != 0 && frameData.m_fireDamage > 12 && SaveData2.saveData.disastersEnabled)
            {
                fs.ExtraFireSpread(buildingID, ref buildingData, 45, this.m_info.m_size.y);
            }

            if ((buildingData.m_flags & Building.Flags.BurnedDown) != Building.Flags.None || (buildingData.m_flags & Building.Flags.Abandoned) != Building.Flags.None)
            {
                float radius = (float)(buildingData.Width + buildingData.Length) * 32.0f;
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Abandonment, 40, buildingData.m_position, radius);
            }
            else if (buildingData.m_fireIntensity == 0)
            {
                int income = 0;
                int tourists = 0;
                CitizenHelper.instance.GetIncome(buildingID, buildingData, ref income, ref tourists);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[(int)district].m_taxationPolicies;

                if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment) && (SimulationManager.instance.m_currentFrameIndex & 1u) == 1u)
                {                    
                    var extraCrime = 0;
                    if (SaveData2.saveData.DifficultyLevel == DifficultyLevel.DwarfFortress)
                    {
                        extraCrime = 10;
                    }
                    if (income < 0)
                    {
                        extraCrime += 10;
                    }
                    if (income < 1000)
                    {
                        extraCrime += 2;
                    }
                    int taxRate = EconomyManager.instance.GetTaxRate(buildingData.Info.m_class.m_service, buildingData.Info.m_class.m_subService, buildingData.Info.m_class.m_level, taxationPolicies);

                    extraCrime += Math.Max(0,taxRate * 5 - 55);
                    buildingData.m_crimeBuffer = (ushort)Mathf.Min(20000, (int)buildingData.m_crimeBuffer + extraCrime);
                }

                if (income > 0)
                {
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.PrivateIncome, -income, this.m_info.m_class, taxationPolicies);
                }
            }
        }

        public override void ModifyMaterialBuffer(ushort buildingID, ref Building data, TransferManager.TransferReason material, ref int amountDelta)
        {
            switch (material)
            {
                case TransferManager.TransferReason.Worker0:
                case TransferManager.TransferReason.Worker1:
                case TransferManager.TransferReason.Worker2:
                case TransferManager.TransferReason.Worker3:
                    {
                        if (data.m_customBuffer1 == 0)
                        {
                            data.m_customBuffer1 = (ushort)(120 + LevelUpHelper3.instance.GetWealthThreshhold(data.Info.m_class.m_level - 1, data.Info.m_class.GetZone()));
                        }

                        if (amountDelta > 0)
                        {
                            DistrictManager instance = Singleton<DistrictManager>.instance;
                            byte district = instance.GetDistrict(data.m_position);
                            DistrictPolicies.Taxation taxationPolicies = instance.m_districts.m_buffer[(int)district].m_taxationPolicies;
                            int taxRate = Singleton<EconomyManager>.instance.GetTaxRate(this.m_info.m_class, taxationPolicies);
                            amountDelta += (50 - taxRate * 4);
                            amountDelta = Math.Max(2, amountDelta / CalculateHomeCount(data));
                        }
                        else
                        {

                            amountDelta = Math.Min(-2, amountDelta / CalculateHomeCount(data));
                        }

                        data.m_customBuffer1 = (ushort)Mathf.Clamp(data.m_customBuffer1 + amountDelta, 1, 30000);
                        return;
                    }
                case TransferManager.TransferReason.Crime:
                    int crimeBuffer = (int)data.m_crimeBuffer;
			        amountDelta = Mathf.Clamp(amountDelta, -crimeBuffer, 65535 - crimeBuffer) / 4;
			        data.m_crimeBuffer = (ushort)(crimeBuffer + amountDelta);
                    return;
            }
            base.ModifyMaterialBuffer(buildingID, ref data, material, ref amountDelta);
        }

        public static int CalculateHomeCount(Building data)
        {
            var iClass = data.Info.m_class;
            int num;
            if (iClass.m_subService == ItemClass.SubService.ResidentialLow)
            {
                if (iClass.m_level == ItemClass.Level.Level1)
                {
                    num = 20;
                }
                else if (iClass.m_level == ItemClass.Level.Level2)
                {
                    num = 25;
                }
                else if (iClass.m_level == ItemClass.Level.Level3)
                {
                    num = 30;
                }
                else if (iClass.m_level == ItemClass.Level.Level4)
                {
                    num = 35;
                }
                else
                {
                    num = 40;
                }
            }
            else if (iClass.m_level == ItemClass.Level.Level1)
            {
                num = 60;
            }
            else if (iClass.m_level == ItemClass.Level.Level2)
            {
                num = 100;
            }
            else if (iClass.m_level == ItemClass.Level.Level3)
            {
                num = 130;
            }
            else if (iClass.m_level == ItemClass.Level.Level4)
            {
                num = 150;
            }
            else
            {
                num = 160;
            }
            return Mathf.Max(100, data.m_width * data.m_length * num) / 100;
        }

        public override float GetEventImpact(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource, float amount)
        {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
            {
                return 0f;
            }
            float result = LevelUpHelper3.instance.GetEventImpact(buildingID, data, resource, amount);
            if (result != 0)
            {
                return result;
            }
            else
            {
                return base.GetEventImpact(buildingID, ref data, resource, amount);
            }
        }

    }

}
