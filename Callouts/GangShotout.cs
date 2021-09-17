using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TioneV.Callouts.Callouts
{
    [CalloutInfo("GangShotout", CalloutProbability.Low)]
    public class GangShotout : Callout
    {
        private readonly static Vector3 GrooveStreet = new Vector3(91.91774f, -1929.053f, 20.8039f);
        private List<Ped> GangA;
        private List<Ped> GangB;
        private List<Blip> Blips;
        private Blip GrooveStreetBlip;
        private bool CalloutActive;
        private bool PursuitStarted;
        private LHandle Pursuit;
        private bool OnScene;

        public override bool OnBeforeCalloutDisplayed()
        {
            base.CalloutPosition = GrooveStreet;
            base.CalloutMessage = $"All units shots fired on Groove Street, posible gang shotout. Response Code 3.";
            base.ShowCalloutAreaBlipBeforeAccepting(base.CalloutPosition, 80f);

            GrooveStreetBlip = new Blip(GrooveStreet)
            {
                Color = Color.Red,
            };
            GrooveStreetBlip.EnableRoute(Color.Red);
            Blips = new List<Blip>();

            Random rnd = new Random();

            var weapons = new string[] { "WEAPON_MICROSMG", "WEAPON_ASSULTRIFLE" };

            var gangAModels = new string[] { "g_m_y_ballaorig_01", "g_m_y_ballaeast_01", "g_f_y_ballas_01", "ig_ballasog", "g_m_y_ballasout_01" };
            GangA = new List<Ped>()
            {
                new Ped(gangAModels[rnd.Next(0, 4)], new Vector3(126.156f, -1926.276f, 24.69776f), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], new Vector3(120.1866f, -1943.645f, 20.75132f), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], new Vector3(84.88507f, -1947.229f, 20.77503f), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], new Vector3(119.2197f, -1953.168f, 24.13887f), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], new Vector3(76.8318f, -1931.885f, 20.80873f), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(GrooveStreet.Around(3f)), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(GrooveStreet.Around(3f)), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(GrooveStreet.Around(3f)), 1),
                new Ped(gangAModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(GrooveStreet.Around(3f)), 1),
            };
            var gangBModels = new string[] { "g_f_y_vagos_01", "g_m_y_salvaboss_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03" };
            GangB = new List<Ped>()
            {
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
                new Ped(gangBModels[rnd.Next(0, 4)], World.GetNextPositionOnStreet(new Vector3(82.64914f, -1894.371f, 22.33472f).Around(2f)), 1),
            };

            Game.SetRelationshipBetweenRelationshipGroups("GANG_A", RelationshipGroup.Player, Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Player, "GANG_A", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.Cop, "GANG_A", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("GANG_A", RelationshipGroup.Cop, Relationship.Hate);

            Game.SetRelationshipBetweenRelationshipGroups("GANG_A", RelationshipGroup.AmbientGangBallas, Relationship.Respect);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.AmbientGangBallas, "GANG_A", Relationship.Respect);

            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.AmbientGangBallas, "GANG_B", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("GANG_B", RelationshipGroup.AmbientGangBallas, Relationship.Hate);

            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.AmbientGangMexican, "GANG_A", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("GANG_A", RelationshipGroup.AmbientGangMexican, Relationship.Hate);

            Game.SetRelationshipBetweenRelationshipGroups("GANG_B", RelationshipGroup.AmbientGangMexican, Relationship.Respect);
            Game.SetRelationshipBetweenRelationshipGroups(RelationshipGroup.AmbientGangMexican, "GANG_B", Relationship.Respect);

            Game.SetRelationshipBetweenRelationshipGroups("GANG_A", "GANG_B", Relationship.Hate);
            Game.SetRelationshipBetweenRelationshipGroups("GANG_B", "GANG_A", Relationship.Hate);

            foreach (var gangAPed in GangA)
            {
                gangAPed.Health = 120;
                gangAPed.Armor = 120;
                gangAPed.Inventory.GiveNewWeapon(weapons[rnd.Next(0, (weapons.Length - 1))], 1256, true);
                gangAPed.RelationshipGroup = "GANG_A";
                gangAPed.CanAttackFriendlies = false;
            }

            foreach (var gangBPed in GangB)
            {
                gangBPed.Health = 120;
                gangBPed.Armor = 120;
                gangBPed.Inventory.GiveNewWeapon(weapons[rnd.Next(0, (weapons.Length - 1))], 1256, true);
                gangBPed.RelationshipGroup = "GANG_B";
                gangBPed.CanAttackFriendlies = false;
            }

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            Functions.PlayScannerAudioUsingPosition("DISPATCH_INTRO_02 ATTENTION_ALL_UNITS_01 WE_HAVE CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_01 OUTRO_01", GrooveStreet);

            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            CalloutActive = true;

            foreach (var gangAPed in GangA)
            {
                gangAPed.Tasks.FightAgainstClosestHatedTarget(600f);
            }

            foreach (var gangBPed in GangB)
            {
                gangBPed.Tasks.FightAgainstClosestHatedTarget(600f);
            }

            CalloutHandler();

            return base.OnCalloutAccepted();
        }

        private void CalloutHandler()
        {
            GameFiber.StartNew(delegate
            {
                Functions.PlayScannerAudio("OFFICER_INTRO_02 UNIT_RESPONDING_DISPATCH_03 OUTRO_03");
                GameFiber.Sleep(3000);
                Functions.PlayScannerAudio("REPORT_RESPONSE_COPY_01 OUTRO_01");

                while (CalloutActive)
                {
                    try
                    {
                        if (!PursuitStarted && !OnScene && Vector3.Distance(Game.LocalPlayer.Character.Position, GrooveStreet) < 100f)
                        {
                            OnScene = true;
                            if (GrooveStreetBlip.Exists())
                                GrooveStreetBlip.Delete();

                            Functions.PlayScannerAudio("DISPATCH_INTRO_01 OFFICERS_ARRIVED_ON_SCENE OUTRO_01");
                            GameFiber.Sleep(2000);
                            Functions.PlayScannerAudio("IN_02 REQUEST_BACKUP_01 OUTRO_02");
                            GameFiber.Sleep(2000);
                            Functions.PlayScannerAudioUsingPosition("DISPATCH_INTRO_02 CRIME_ASSAULT_PEACE_OFFICER_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_99_01 OUTRO_01", GrooveStreet);
                            GameFiber.Sleep(5000);
                            Functions.RequestBackup(GrooveStreet, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.SwatTeam);
                            Functions.RequestBackup(GrooveStreet, LSPD_First_Response.EBackupResponseType.Code3, LSPD_First_Response.EBackupUnitType.LocalUnit);
                        }

                        if (!PursuitStarted && (GangB.Count(bPed => bPed.Exists() && bPed.IsAlive && !Functions.IsPedArrested(bPed)) < 3 || GangA.Count(aPed => aPed.Exists() && aPed.IsAlive && !Functions.IsPedArrested(aPed)) < 3))
                        {
                            PursuitStarted = true;
                            Pursuit = Functions.CreatePursuit();
                            if (GrooveStreetBlip.Exists())
                                GrooveStreetBlip.Delete();

                            Functions.SetPursuitIsActiveForPlayer(Pursuit, true);

                            foreach (var gangaPed in GangA.Where(a => a.IsAlive && !Functions.IsPedArrested(a)))
                            {
                                Functions.AddPedToPursuit(Pursuit, gangaPed);
                                GameFiber.Yield();
                            }
                            foreach (var gangbPed in GangB.Where(b => b.IsAlive && !Functions.IsPedArrested(b)))
                            {
                                Functions.AddPedToPursuit(Pursuit, gangbPed);
                                GameFiber.Yield();
                            }

                            Functions.RequestBackup(GrooveStreet, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.AirUnit);
                        }
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial(ex.ToString());
                        Game.DisplayNotification($"BUG! {ex.Message} - Force Callout End.");
                        End();
                    }
                    finally
                    {
                        GameFiber.Yield();
                    }
                }
            });
        }

        public override void Process()
        {
            base.Process();

            if (PursuitStarted && !Functions.IsPursuitStillRunning(Pursuit))
                GameFiber.StartNew(End);
            else if (GangA.Where(a => a.Exists()).All(a => a.IsDead || Functions.IsPedArrested(a)) && GangB.Where(b => b.Exists()).All(b => b.IsDead || Functions.IsPedArrested(b)))
                GameFiber.StartNew(End);
            else if (Game.LocalPlayer.Character.IsDead)
                GameFiber.StartNew(End);
        }

        public override void End()
        {
            CalloutActive = false;
            PursuitStarted = false;
            OnScene = false;

            if (Game.LocalPlayer.Character.IsDead)
                Functions.PlayScannerAudio("DISPATCH_INTRO_01 OFFICER HAS_BEEN_FATALLY_SHOT NOISE_SHORT OFFICER_NEEDS_IMMEDIATE_ASSISTANCE");
            else
                Functions.PlayScannerAudio("DISPATCH_INTRO_01 WE_ARE_CODE_4 NO_FURTHER_UNITS_REQUIRED");

            GameFiber.Wait(2000);

            base.End();

            GangA?.Where(ped => ped.Exists())?.ToList()?.ForEach(ped => ped.Dismiss());
            GangB?.Where(ped => ped.Exists())?.ToList()?.ForEach(ped => ped.Dismiss());
            Blips?.Where(blip => blip.Exists())?.ToList()?.ForEach(blip => blip.Delete());
            if (GrooveStreetBlip?.Exists() ?? false) GrooveStreetBlip.Delete();
        }
    }
}
