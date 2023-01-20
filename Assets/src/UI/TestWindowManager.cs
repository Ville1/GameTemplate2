using Game.Input;
using Game.Utils;
using UnityEngine;

namespace Game.UI
{
    public class TestWindowManager : WindowBase
    {
        public static TestWindowManager Instance;

        public TemplateObjectElement TemplateObjectElement;

        private Timer counterTimer = null;
        private ExampleObject exampleObject = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            AutoAssignCloseButton = true;
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.F11, () => { ToggleActive(); }, KeyEventTag.IgnoreUI);
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

        protected override void OnOpen()
        {
            exampleObject = new ExampleObject();
            TemplateObjectElement.Link(exampleObject);
            counterTimer = new Timer(1.0f, () => { exampleObject.Increment(); });
        }
    }
}