using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using alexguirre.Common.Extensions;

namespace TioneV.Callouts.Callouts
{
    [CalloutInfo("DrunkDriver", CalloutProbability.High)]
    public class DrunkDriver : Callout
    {
        private Ped SuspectDriver;
        private Ped SuspectPasanger;
        private Vehicle SuspectVehicle;
        private Group SusspectsGroup;
        private string SuspectVehicleName;
        private LHandle Pursuit;
        private List<Blip> SuspectsBlips;
        private Vector3 SpawnPoint;
        private bool PursuitCreated;
        private bool SuspectEngange;

        private string[] SUSPECT_CARS = new[] { "INTRUDER", "PRIMO2", "EMPEROR2", "BALLER", "REBEL2", "SANDKING", "STALION" };

        public override bool OnBeforeCalloutDisplayed()
        {
            SpawnPoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(300f, 1500f));

            ShowCalloutAreaBlipBeforeAccepting(SpawnPoint, 30f);
            AddMinimumDistanceCheck(20f, SpawnPoint);

            CalloutMessage = "Driving under influence - Suspects are armed";
            CalloutPosition = SpawnPoint;

            Random rndCars = new Random();
            SuspectVehicleName = SUSPECT_CARS[rndCars.Next(0, (SUSPECT_CARS.Length - 1))];

            Functions.PlayScannerAudioUsingPosition($"WE_HAVE CRIME_ASSAULT_WITH_A_DEADLY_WEAPON_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_03_01 OUTRO_01", SpawnPoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override void OnCalloutDisplayed()
        {
            base.OnCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            try
            {
                SuspectsBlips = new List<Blip>();

                SuspectVehicle = new Vehicle(SuspectVehicleName, SpawnPoint);
                SuspectVehicle.IsPersistent = true;

                Functions.PlayScannerAudioUsingPosition($"SUSPECT_LAST_SEEN_01 IN_A_01 COLOR_{SuspectVehicle.GetColors().PrimaryColorName.ToUpper()}_01 {SuspectVehicleName}_01 IN_OR_ON_POSITION OUTRO_01", SpawnPoint);

                SuspectDriver = SuspectVehicle.CreateRandomDriver();
                SuspectDriver.Inventory.GiveNewWeapon("WEAPON_COMBATPISTOL", 60, true);
                SuspectDriver.IsPersistent = true;
                SuspectDriver.CanAttackFriendlies = false;
                SuspectDriver.RelationshipGroup = "SUSPECTS";
                SuspectDriver.BlockPermanentEvents = true;
                SuspectDriver.Armor = 100;
                SuspectsBlips.Add(SuspectDriver.AttachBlip());
                SuspectsBlips[0].IsFriendly = false;
                SuspectDriver.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);
                NativeFunction.Natives.SetPedCombatAbility(SuspectDriver, 2);
                Game.LogTrivial("Driver created");

                SusspectsGroup = new Group(SuspectDriver);

                SuspectPasanger = new Ped(SpawnPoint);
                SuspectPasanger.Inventory.GiveNewWeapon("WEAPON_MICROSMG", 600, true);
                SuspectPasanger.IsPersistent = true;
                SuspectPasanger.CanAttackFriendlies = false;
                SuspectDriver.RelationshipGroup = "SUSPECTS";
                SuspectPasanger.BlockPermanentEvents = true;
                SuspectPasanger.Armor = 200;
                SuspectPasanger.WarpIntoVehicle(SuspectVehicle, -2);
                SuspectPasanger.Tasks.FightAgainstClosestHatedTarget(100f);
                SuspectsBlips.Add(SuspectPasanger.AttachBlip());
                SuspectsBlips[1].IsFriendly = false;
                NativeFunction.Natives.SetPedCombatAbility(SuspectPasanger, 3);
                Game.LogTrivial("Pasanger created");

                SuspectsBlips.Add(SuspectVehicle.AttachBlip());
                SuspectsBlips[2].EnableRoute(Color.Red);
                SuspectsBlips[2].Color = Color.Red;

                SusspectsGroup.AddMember(SuspectPasanger);
                SusspectsGroup.Formation.DistanceToLeader = 1f;
                SusspectsGroup.Formation.Type = GroupFormationType.Circle;

                Game.SetRelationshipBetweenRelationshipGroups("SUSPECTS", "PLAYER", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("PLAYER", "SUSPECTS", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("COP", "SUSPECTS", Relationship.Hate);
                Game.SetRelationshipBetweenRelationshipGroups("SUSPECTS", "COP", Relationship.Hate);
            }
            catch (Exception ex)
            {
                Game.LogTrivial(ex.ToString());
                End();
            }

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(SuspectVehicle.Position) < 100f)
            {
                Game.DisplayNotification($"Suspect vehicle {SuspectVehicle.LicensePlate}, starting pursuit.");
                Functions.PlayScannerAudio($"OFFICER_INTRO_02 REPORT_SUSPECT_IS_IN_CAR_01 OUTRO_02");

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, SuspectDriver);
                Functions.AddPedToPursuit(Pursuit, SuspectPasanger);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                PursuitCreated = true;
            }

            if (PursuitCreated && SuspectsBlips.Count > 2 && SuspectsBlips[2].Exists())
            {
                SuspectsBlips[2].Delete();
                SuspectsBlips.RemoveAt(2);
            }

            if ((!SuspectDriver.IsCuffed && SuspectDriver.IsAlive) && !SuspectDriver.IsInAnyVehicle(true))
            {
                SuspectDriver.Tasks.FightAgainstClosestHatedTarget(100f);
                if (Game.LocalPlayer.Character.DistanceTo(SuspectDriver.Position) < 100f)
                    NativeFunction.CallByName<uint>("TASK_COMBAT_PED", SuspectDriver, Game.LocalPlayer.Character, 0, 1);
            }

            if ((!SuspectPasanger.IsCuffed && SuspectPasanger.IsAlive) && !SuspectPasanger.IsInAnyVehicle(true))
            {
                SuspectPasanger.Tasks.FightAgainstClosestHatedTarget(300f);
                if (Game.LocalPlayer.Character.DistanceTo(SuspectPasanger.Position) < 100f)
                    NativeFunction.CallByName<uint>("TASK_COMBAT_PED", SuspectPasanger, Game.LocalPlayer.Character, 0, 1);
            }

            if (!SuspectEngange && (SuspectDriver.IsShooting || SuspectPasanger.IsShooting))
            {
                SuspectEngange = true;

                Functions.PlayScannerAudioUsingPosition("ATTENTION_ALL_UNITS_04 CRIME_SHOTS_FIRED_AT_AN_OFFICER_03 IN_OR_ON_POSITION UNITS_RESPOND_CODE_99_01 OUTRO_01", Game.LocalPlayer.Character.Position);
                Functions.RequestBackup(Game.LocalPlayer.Character.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.SwatTeam);
            }

            if (!SuspectDriver.Exists()) SuspectsBlips[0].Delete();
            else if (Functions.IsPedArrested(SuspectDriver)) SuspectsBlips[0].Color = Color.Orange;
            else if (SuspectDriver.IsDead) SuspectsBlips[0].Color = Color.Black;

            if (!SuspectPasanger.Exists()) SuspectsBlips[1].Delete();
            else if (Functions.IsPedArrested(SuspectPasanger)) SuspectsBlips[1].Color = Color.Orange;
            else if (SuspectPasanger.IsDead) SuspectsBlips[1].Color = Color.Black;

            if ((SuspectDriver.IsDead || Functions.IsPedArrested(SuspectDriver) && (SuspectPasanger.IsDead || Functions.IsPedArrested(SuspectPasanger)) || (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit))))
                End();
        }

        public override void End()
        {
            Game.LogTrivial("End callout");

            PursuitCreated = false;
            SuspectEngange = false;
            SuspectVehicleName = null;

            base.End();

            if (!Game.LocalPlayer.IsDead)
                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH WE_ARE_CODE FOUR NO_FURTHER_UNITS_REQUIRED OUTRO_01");

            if (SuspectDriver?.Exists() ?? false) SuspectDriver.Dismiss();
            if (SuspectPasanger?.Exists() ?? false) SuspectPasanger.Dismiss();
            if (SuspectVehicle?.Exists() ?? false) SuspectVehicle.Dismiss();
            if (SusspectsGroup?.Exists() ?? false) SusspectsGroup.Delete();
            SuspectsBlips?.Where(blip => blip.Exists())?.ToList()?.ForEach(blip => blip.Delete());
        }
    }
}
