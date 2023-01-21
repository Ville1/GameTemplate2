using Game.Input;
using Game.UI.Components;
using Game.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TestWindowManager : WindowBase
    {
        public static TestWindowManager Instance;

        public TemplateObjectElement TemplateObjectElement;
        public GameObject GridViewScrollView;
        public Button TestButton;

        private Timer counterTimer = null;
        private ExampleObject exampleObject = null;
        private GridView gridView = null;
        private CustomButton testButton;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            AutoAssignCloseButton = true;
            AutoAssignAcceptButton = true;
            AutoAssignCancelButton = true;
            AcceptEnabled = true;
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.F11, () => { ToggleActive(); }, KeyEventTag.IgnoreUI);

            gridView = new GridView(GridViewScrollView, new GridViewParameters() { MaxHeight = 3, FillOrder = GridView.GridFillOrder.Vertical });
            testButton = new CustomButton(TestButton, null, TestAction);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (Active) {
                counterTimer.Update();
            }
        }

        private void TestAction()
        {
            gridView.AddCell(new List<UIElementData>() { UIElementData.Text("Text (TMP)", "A", null) }, new Coordinates(1, 2));
        }

        protected override void OnOpen()
        {
            exampleObject = new ExampleObject();
            TemplateObjectElement.Link(exampleObject);
            counterTimer = new Timer(1.0f, () => { exampleObject.Increment(); });

            gridView.AddCell(new List<UIElementData>() { UIElementData.Text("Text (TMP)", "1", null) });
            gridView.AddCell(new List<UIElementData>() { UIElementData.Text("Text (TMP)", "2", null) });
            gridView.AddCell(new List<UIElementData>() { UIElementData.Text("Text (TMP)", "3", null) });

            CustomLogger.Debug("OPEN");
        }

        protected override void OnClose()
        {
            gridView.Clear();
            CustomLogger.Debug("CLOSE");
        }

        protected override void OnAccept()
        {
            CustomLogger.Debug("ACCEPT");
        }

        protected override void OnCancel()
        {
            CustomLogger.Debug("CANCEL");
        }
    }
}