﻿using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Math;
using GTA.Native;

namespace Test
{
    internal class LifeSimulator : Script
    {
        private Ped[] _peds;
        private int _count;
        private bool _playerChange;
        private Model _playerModel;

        public LifeSimulator()
        {
            Tick += OnTick;
            Interval = 1000;
        }

        private int Eat { get; set; } = 100;
        public static bool On { get; set; }

        private void OnTick(object sender, EventArgs e)
        {
            if (!On && _count > 0)
            {
                Game.Player.ChangeModel(_playerModel);
                Game.Player.Character.Task.ClearAll();
                _playerChange = false;
                _count = 0;
                Eat = 100;
            }
            if (On)
            {
                PlayerChange();
                Hunt();
                Eating();

                //if (count == 2)
                //{
                //    Hunt();
                //    count = 0;
                //}


                UI.ShowSubtitle("Life Engine: " + _count +
                                "\nEat: " + Eat);

                if (_count % 4 == 0) Eat--;
                _count++;
            }
        }

        void Eating()
        {
            _peds = World.GetNearbyPeds(Game.Player.Character.Position, 10f);
            foreach (var ped in _peds)
            {
                if (!ped.IsInVehicle() &&
                    World.GetDistance(Game.Player.Character.Position, ped.Position)
                    < 1.5f && !ped.IsPlayer)
                {
                    if (ped.IsAlive)
                    {
                        Eat += 10;
                        UI.Notify("Eating + 10 ");

                        ped.Task.HandsUp(10000);
                        CH.HPeds.PedDamageHeavy(ped);
                        Game.Player.Character.Task.RunTo(ped.Position);
                        ped.Kill();
                        ped.MarkAsNoLongerNeeded();
                        if (ped.Model == new Model(PedHash.Seagull) ||
                            ped.Model == new Model(PedHash.Rat))
                        {
                            ped.Delete();
                        }
                    }
                }
            }
        }

        void PlayerChange()
        {
            if (!_playerChange)
            {
                _playerModel = Game.Player.Character.Model;
                CH.HPeds.ChangePlayerModel(GTA.Native.PedHash.MountainLion);
                _playerChange = true;
            }
        }

        private Blip blip;
        void Hunt()
        {
            _peds = World.GetNearbyPeds(Game.Player.Character.Position +
                Game.Player.Character.ForwardVector * 5, 100f);
            foreach (var ped in _peds)
            {
                //ped.Task.FightAgainst(Game.Player.Character, 20000);
                if (!ped.IsInVehicle() && !ped.IsInAir && !ped.IsPlayer && !ped.IsDead && _count % 3 == 0)
                {
                    blip?.Remove();

                    if (ped.IsRunning)
                    {
                        blip = World.CreateBlip(ped.Position + ped.ForwardVector * 15);
                        blip.Color = BlipColor.Blue;
                        Game.Player.Character.Task.RunTo(ped.Position + ped.ForwardVector * 15);
                    }

                    else
                    {
                        blip = World.CreateBlip(ped.Position);
                        blip.Color = BlipColor.Yellow;
                        Game.Player.Character.Task.RunTo(ped.Position);
                    }
                }
                else if (_count % 6 == 0)
                {
                    Game.Player.Character.Task.WanderAround();
                }
            }
        }
    }
}
