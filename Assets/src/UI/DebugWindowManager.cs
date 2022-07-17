using Game.UI.Components;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class DebugWindowManager : WindowBase
    {
        public static DebugWindowManager Instance;

        public GameObject ListContainer;

        private UIList list;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;
            Tags.Add(Tag.HUD);
            list = new UIList(ListContainer);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public override bool Active {
            get {
                return base.Active;
            }
            set {
                base.Active = value;
                if (base.Active) {
                    list.Clear();
                    Height = 10.0f;
                }
            }
        }

        public void SetValue(string name, string value)
        {
            if (!Active) {
                return;
            }
            if (list.HasRow(name)) {
                list.SetRow(name, new List<UIElementData>() { UIElementData.Text("Text", string.Format("{0}: {1}", name, value), null) });
            } else {
                list.AddRow(name, new List<UIElementData>() { UIElementData.Text("Text", string.Format("{0}: {1}", name, value), null) });
                Height = list.Height + 10.0f;
            }
        }
    }
}
