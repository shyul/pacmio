﻿/// ***************************************************************************
/// Pacmio Research Enivironment
/// Copyright 2001-2008, 2014-2021 Xu Li - me@xuli.us
/// 
/// ***************************************************************************

using System.Collections.Generic;
using System.Drawing;
using Xu;

namespace TestClient
{
    public sealed partial class ChartTestForm : MosaicForm
    {
        public ChartTestForm() : base(Program.SHOW_PACMIO)
        {
            SuspendLayout();

            HelpLink = "https://github.com/shyul/Pacmio/wiki";
            Icon = Pacmio.Properties.Resources.Pacman;
            /*
            RibbonButton rbtn_1 = new RibbonButton(c_IBHistorial, 0, Importance.Major);
            RibbonPane rbtpane_IBClient = new RibbonPane("IBClient Test", 0);
            rbtpane_IBClient.Add(rbtn_1);
            RibbonTabItem rbtIBClient = new RibbonTabItem("IBClient");
            rbtIBClient.Add(rbtpane_IBClient, 0);
            Ribbon.AddRibbonTab(rbtIBClient);
            */
            ResumeLayout(false); // false
            PerformLayout();

        }

        private readonly Command c_IBHistorial = new()
        {
            //Enabled = false,
            Label = "Historical Data Request Awseome with more Awesome!!",

            IconList = new Dictionary<IconType, Dictionary<Size, Bitmap>>() { { IconType.Normal, new Dictionary<Size, Bitmap>() {
                    { new Size(16, 16), Xu.Properties.Resources.Database_16 },
                    { new Size(24, 24), Xu.Properties.Resources.Database_24 },
                    { new Size(32, 32), Xu.Properties.Resources.Database_32 }
                } } },
            //Action = () => { TestPublicHistoricalData(); },
        };
    }
}
