using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;

namespace TioneV.Callouts
{
    public class Main : Plugin
    {
        //For further information and explanation please check the PDF file.
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial("TioneV.Callouts " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " by kubala156 has been initialised.");
        }
        public override void Finally()
        {

        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.DrunkDriver));
            Functions.RegisterCallout(typeof(Callouts.GangShotout));
        }
    }
}
