using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;
using System.Runtime.InteropServices;
using System.IO;
namespace SniperMod
{
    public class SniperMode : BaseScript
    {
        public SniperMode()
            : base()
        {
            this.changeGametype("^2LSDSnipe");
            //changeGametype("^2LSDSnipe 1.1"); 
            Log.Write(LogLevel.All, "**********************************");
            Log.Write(LogLevel.All, "SniperMod by LSD!");
            Log.Write(LogLevel.All, "Create by Modder");
            Log.Write(LogLevel.All, "Job: Hardscope,AntiCamp and AntiKniefe");
            Log.Write(LogLevel.All, "Help: zxz0O0, Nukem, Jariz, Pozzuh, Makavel und DaMacc");
            Log.Write(LogLevel.All, "**********************************");
            antiHardScope(0.1f);

            //NoKnife nk = new NoKnife();
            IW5MExtensions.NoKnife nk = new IW5MExtensions.NoKnife();
            nk.DisableKnife();
            antiCamp(10f);
            //PingCheck(60f);   
        }

        public void OnPlayerSpawn(Entity entity)
        {
            
        }
        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            if (mod == "MOD_FALLING")
            {
                return;
            }
            if (!player.IsPlayer)
            {
                return;
            }
            if (player == attacker)
            {
                return;
            }
            if (player.GetField<string>("sessionteam") != attacker.GetField<string>("sessionteam") && attacker.Origin.DistanceTo2D(player.Origin) < 150f && !player.CurrentWeapon.Contains("scope") && attacker.Call<float>("playerads", new Parameter[0]) == 0f)
            {
                player.Health += damage;
                attacker.Call("iprintlnbold", new Parameter[]
				{
					"^2Noscope ist auf diesem Server nicht erlaubt!"
				});
            }
        }
        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (mod == "MOD_FALLING")
            {
                return;
            }
            if (!player.IsPlayer)
            {
                return;
            }
            if (player == attacker)
            {
                return;
            }
            if (player.GetField<string>("sessionteam") != attacker.GetField<string>("sessionteam") && attacker.Origin.DistanceTo2D(player.Origin) < 150f && !player.CurrentWeapon.Contains("scope") && attacker.Call<float>("playerads", new Parameter[0]) == 0f)
            {
                attacker.Call("suicide");
                attacker.Call("iprintlnbold", new Parameter[]
				{
					"^2Noscope ist auf diesem Server nicht erlaubt!"
				});
                
            }
        }
        private void PingCheck(float checktime)
        {
            Log.Write(LogLevel.All, "PingCheck Started!");
            PlayerConnecting += new Action<Entity>(entity =>
            {
                entity.OnInterval((int)(checktime * 1000), player =>
                {
                    if (player.Ping > 250)
                    {
                        Utilities.ExecuteCommand("dropclient " + player.Call<int>("getentitynumber", new Parameter[0]) + " \"To heigh ping! You Ping: " + player.Ping + "ms/250ms\"");
                        string text4 = "^2<playername> ^3has been kicked ^7for ^1<kicker>";
                        text4 = text4.Replace("<playername>", player.Name.ToString());
                        text4 = text4.Replace("<kicker>", "To heigh ping: " + player.Ping + "ms/250ms");
                        Utilities.ExecuteCommand("say " + text4);
                        return true;
                    }
                    return true;
                });
            });
        }

        private void antiHardScope(float maxtime)
        {
            Log.Write(LogLevel.All, "AntiHardScope Started!");
            PlayerConnecting += new Action<Entity>(entity =>
            {
                int adsTime = 0;
                entity.OnInterval(100, player =>
                {
                    if (!player.IsAlive)
                    {
                        return true;
                    }
                    if (player.Call<float>("playerads") >= 1)
                    {
                        adsTime++;
                    }
                    else
                    {
                        adsTime = 0;
                    }

                    if (adsTime >= maxtime * 10)
                    {
                        adsTime = 0;
                        player.Call("allowads", false);
                        OnInterval(50, () =>
                        {
                            if (player.Call<int>("adsbuttonpressed") > 0)
                            {
                                return true;
                            }
                            player.Call("allowads", true);
                            return false;
                        });
                    }
                    return true;
                });
            });
        }

        private void antiCamp(double waittime)
        {
            Log.Write(LogLevel.All, "AntiCamp Started!");
            PlayerConnecting += new Action<Entity>(entity =>
            {
                entity.OnInterval((int)(waittime * 1000), player =>
                {
                    if (!player.IsAlive || player.Call<int>("istalking") != 0)//|| !_prematchDone)
                    {
                        return true;
                    }
                    if (player.HasField("ac_lastPos"))
                    {
                        Vector3 lastPos = player.GetField<Vector3>("ac_lastPos");

                        if (lastPos.DistanceTo2D(player.Origin) < 100)
                        {
                            int i = 5;
                            OnInterval(1000, () =>
                            {
                                if (lastPos.DistanceTo2D(player.Origin) > 150) return false;
                                if (!player.IsAlive) return false;
                                if (i == 5)
                                {
                                    player.Call("iprintlnbold", "^2ANTICAMP:^7 You will be killed in^1 5sec");
                                    i--;
                                    return true;
                                }
                                else if (i > 0)
                                {
                                    player.Call("iprintlnbold", "^2ANTICAMP:^7 You will be killed in^1 " + i + "sec");
                                    i--;
                                    return true;
                                }
                                else
                                {
                                    player.Call("suicide");
                                }
                                return false;
                            });
                        }
                    }

                    player.SetField("ac_lastPos", player.Origin);
                    return true;
                });
            });
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

        public IntPtr alloc(int size)
        {
            return VirtualAlloc(IntPtr.Zero, (UIntPtr)size, 0x3000, 0x40);
        }

        public bool unalloc(IntPtr address, int size)
        {
            return VirtualFree(address, (UIntPtr)size, 0x8000);
        }

        bool _changed = false;
        IntPtr memory;
        private unsafe void changeGametype(string gametype)
        {
            byte[] gametypestring;
            if (_changed)
            {
                gametypestring = new System.Text.UTF8Encoding().GetBytes(gametype);
                if (gametypestring.Length >= 64) gametypestring[64] = 0x0; // null terminate if too large
                Marshal.Copy(gametypestring, 0, memory, gametype.Length > 64 ? 64 : gametype.Length);
                return;
            }
            memory = alloc(64);
            gametypestring = new System.Text.UTF8Encoding().GetBytes(gametype);
            if (gametypestring.Length >= 64) gametypestring[64] = 0x0; // null terminate if too large
            Marshal.Copy(gametypestring, 0, memory, gametype.Length > 64 ? 64 : gametype.Length);
            *(byte*)0x4EB983 = 0x68; // mov eax, 575D928h -> push stringloc
            *(int*)0x4EB984 = (int)memory;
            *(byte*)0x4EB988 = 0x90; // mov ecx, [eax+0Ch] -> nop
            *(byte*)0x4EB989 = 0x90;
            *(byte*)0x4EB98A = 0x90;
            *(byte*)0x4EB98B = 0x90; // push edx -> nop
            _changed = true;
        }

    }
}

namespace IW5MExtensions
{
    unsafe class NoKnife
    {

        public unsafe void DisableKnife()
        {
            *this.KnifeRange = (int)ZeroAddress;
        }

        public unsafe void EnableKnife()
        {
            *this.KnifeRange = (int)DefaultKnifeAddress;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);


        private int DefaultKnifeAddress;
        private int* KnifeRange;
        private int* ZeroAddress;

        private unsafe int FindMem(byte?[] search, int num = 1, int start = 0x1000000, int end = 0x3d00000)
        {
            byte* num2 = (byte*)0;
            try
            {
                int num3 = 0;
                for (int i = start; i < end; i++)
                {
                    num2 = (byte*)i;
                    bool flag = false;
                    for (int j = 0; j < search.Length; j++)
                    {
                        if (search[j].HasValue)
                        {
                            byte num7 = *num2;
                            if (num7 != search[j])
                            {
                                break;
                            }
                        }
                        if (j == (search.Length - 1))
                        {
                            if (num == 1)
                            {
                                flag = true;
                            }
                            else
                            {
                                num3++;
                                if (num3 == num)
                                {
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            num2++;
                        }
                    }
                    if (flag)
                    {
                        return i;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Write(LogLevel.Error, "FindMem: " + exception.Message + "\nAddress: " + (int)num2);
            }
            return 0;
        }



        public unsafe NoKnife()
        {

            //Log.Write(LogLevel.Info, "NoKnife Plugin loaded \n Author: zxz0O0 \n Thanks to Nukem, Jariz, Pozzuh and Makavel\n Modified for IW5M by DaMacc");

            try
            {

                byte?[] search = new byte?[] { 
                0x8b, null, null, null, 0x83, null, 4, null, 0x83, null, 12, 0xd9, null, null, null, 0x8b, 
                null, 0xd9, null, null, null, 0xd9, 5
             };
                this.KnifeRange = (int*)(this.FindMem(search, 1, 0x400000, 0x500000) + search.Length);

                Log.Write(LogLevel.Info, "KnifeRange ptr: " + string.Format("{0:X}", (int)KnifeRange));

                if ((int)this.KnifeRange == search.Length)
                {
                    byte?[] nullableArray2 = new byte?[] { 
                    0x8b, null, null, null, 0x83, null, 0x18, null, 0x83, null, 12, 0xd9, null, null, null, 0x8d, 
                    null, null, null, 0xd9, null, null, null, 0xd9, 5
                 };
                    this.KnifeRange = (int*)(this.FindMem(nullableArray2, 1, 0x400000, 0x500000) + nullableArray2.Length);

                    Log.Write(LogLevel.Info, "KnifeRange ptr: " + string.Format("{0:X}", (int)KnifeRange));

                    if ((int)this.KnifeRange == nullableArray2.Length)
                    {
                        this.KnifeRange = (int*)0;
                    }
                }
                this.DefaultKnifeAddress = *this.KnifeRange;
                byte?[] nullableArray3 = new byte?[] { 
                0xd9, 0x5c, null, null, 0xd8, null, null, 0xd8, null, null, 0xd9, 0x5c, null, null, 0x83, null, 
                1, 15, 0x86, null, 0, 0, 0, 0xd9
             };
                this.ZeroAddress = (int*)(this.FindMem(nullableArray3, 1, 0x400000, 0x500000) + nullableArray3.Length + 2);

                Log.Write(LogLevel.Info, "ZeroAddress ptr: " + string.Format("{0:X}", (int)ZeroAddress));

                if ((((int)this.KnifeRange == 0) || ((int)this.DefaultKnifeAddress == 0)) || ((int)this.ZeroAddress == 0))
                {
                    Log.Write(LogLevel.Error, "Error finding address: NoKnife Plugin will not work");

                }
                else
                {
                    uint num;
                    VirtualProtect((IntPtr)this.KnifeRange, (IntPtr)4, 0x40, out num);
                }
            }
            catch (Exception exception)
            {
                Log.Write(LogLevel.Error, "Error in NoKnife Plugin. Plugin will not work.");
                Log.Write(LogLevel.Error, exception.ToString());
            }
        }
        
    }
}
