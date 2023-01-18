using Game.Input;
using Game.Utils;
using UnityEngine;

namespace Game.UI
{
    public class TestWindowManager : WindowBase
    {
        public static TestWindowManager Instance;

        public TemplateObjectElement TemplateObjectElement;

        private float counterCooldown = 0.0f;
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
                counterCooldown -= Time.deltaTime;
                if (counterCooldown <= 0.0f) {
                    counterCooldown = 1.0f;
                    exampleObject.Increment();
                }
            }
        }

        public override bool Active
        {
            get {
                return base.Active;
            }
            set {
                base.Active = value;
                if (value) {
                    exampleObject = new ExampleObject();
                    TemplateObjectElement.Link(exampleObject);
                }
            }
        }
    }
}