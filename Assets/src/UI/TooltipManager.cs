using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class TooltipManager : MonoBehaviour
    {
        private static readonly float HOVER_TIME = 1.0f;
        private static readonly float MAX_MOUSE_DISTANCE = 0.1f;
        private static readonly string CUSTOM_PANEL_NAME = "Custom Tooltip";
        private static readonly float MARGIN_HORIZONTAL = 10.0f;
        private static readonly float MARGIN_VERTICAL = 5.0f;
        private static readonly float POSITION_DELTA_X = 8.0f;
        private static readonly float POSITION_DELTA_Y = 0.0f;

        public static TooltipManager Instance;

        public GameObject Canvas;
        public GameObject TooltipPanel;
        public TMP_Text TooltipText;

        private GameObject currentTooltipPanel;
        private RectTransform rectTransform;

        private UnityEngine.UI.GraphicRaycaster raycaster;
        private List<Tooltip> tooltips;
        private Tooltip currentTooltip;
        private float timeLeft;
        private Vector3 lastMousePosition = Vector3.zero;

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;
            tooltips = new List<Tooltip>();

            currentTooltipPanel = TooltipPanel;
            rectTransform = TooltipPanel.GetComponent<RectTransform>();
            TooltipPanel.SetActive(false);

            raycaster = Canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            if(tooltips.Count == 0) {
                return;
            }

            Tooltip newTooltip = null;

            Vector3 currentMousePosition = UnityEngine.Input.mousePosition;
            Vector3 tooltipPosition = new Vector3(currentMousePosition.x + POSITION_DELTA_X, currentMousePosition.y + POSITION_DELTA_Y, currentMousePosition.z);
            float distance = Vector3.Distance(lastMousePosition, currentMousePosition);

            if (distance <= MAX_MOUSE_DISTANCE) {
                //Check for hits in UI
                PointerEventData pointerEventData = new PointerEventData(null);
                pointerEventData.position = UnityEngine.Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                raycaster.Raycast(pointerEventData, results);
                if (results.Count > 0) {
                    Tooltip uiTooltip = tooltips.FirstOrDefault(tooltip => results.Any(hit => hit.gameObject == tooltip.Target));
                    if (uiTooltip != null) {
                        newTooltip = uiTooltip;
                    }
                }

                if (newTooltip == null) {
                    //Check for hits in the game world
                    RaycastHit hit;
                    if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out hit)) {
                        Tooltip worldTooltip = tooltips.FirstOrDefault(tooltip => tooltip.Target == hit.transform.gameObject);
                        if (worldTooltip != null) {
                            newTooltip = worldTooltip;
                        }
                    }
                }
            }

            if(currentTooltip != null && newTooltip == null) {
                //Close tooltip
                CloseTooltip();
            } else if(currentTooltip == null && newTooltip != null) {
                //New tooltip
                timeLeft = HOVER_TIME;
            } else if (currentTooltip != null && newTooltip != null && currentTooltip.Target != newTooltip.Target) {
                //Change tooltip
                timeLeft = HOVER_TIME;
                CloseTooltip();
            } else if (currentTooltip != null && newTooltip != null && currentTooltip.Target == newTooltip.Target) {
                //Same tooltip
                if (currentTooltipPanel.activeSelf) {
                    //Tooltip is open, update position
                    currentTooltipPanel.transform.position = tooltipPosition;
                } else {
                    if (timeLeft > 0.0f) {
                        //Tooltip is not yet visible, reduce timeLeft
                        timeLeft = Math.Max(timeLeft - Time.deltaTime, 0.0f);
                    } else {
                        //Show tooltip
                        OpenTooltip(newTooltip, tooltipPosition);
                    }
                }
            }

            currentTooltip = newTooltip;
            lastMousePosition = currentMousePosition;
        }

        public void RegisterTooltip(Tooltip tooltip)
        {
            if(tooltips.Any(t => t.Target == tooltip.Target)) {
                CustomLogger.Warning("{TooltipAlreadyRegistered}", tooltip.Target.name);
                return;
            }
            tooltips.Add(tooltip);
        }

        public bool UnregisterTooltip(Tooltip tooltip)
        {
            return UnregisterTooltip(tooltip.Target);
        }

        public bool UnregisterTooltip(GameObject target)
        {
            Tooltip tooltip = tooltips.FirstOrDefault(t => t.Target == target);
            if (tooltip != null) {
                if(currentTooltip == tooltip) {
                    currentTooltip = null;
                    CloseTooltip();
                }
                tooltips.Remove(tooltip);
                return true;
            }
            return false;
        }

        private void CloseTooltip()
        {
            if(currentTooltipPanel.name == CUSTOM_PANEL_NAME) {
                Destroy(currentTooltipPanel);
                currentTooltipPanel = TooltipPanel;
            }
            currentTooltipPanel.SetActive(false);
        }

        private void OpenTooltip(Tooltip tooltip, Vector3 currentMousePosition)
        {
            if (currentTooltipPanel.activeSelf) {
                CloseTooltip();
            }

            if(tooltip.CustomTooltipPanel == null) {
                currentTooltipPanel.SetActive(true);
                TooltipText.text = currentTooltip.Text;
                currentTooltipPanel.transform.position = currentMousePosition;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentTooltip.Width ?? (TooltipText.preferredWidth + MARGIN_HORIZONTAL));
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentTooltip.Height ?? (TooltipText.preferredHeight + MARGIN_VERTICAL));
            } else {
                TooltipPanel.SetActive(false);
                currentTooltipPanel = Instantiate(
                    tooltip.CustomTooltipPanel,
                    new Vector3(
                        TooltipPanel.transform.position.x,
                        TooltipPanel.transform.position.y,
                        TooltipPanel.transform.position.z
                    ),
                    Quaternion.identity,
                    gameObject.transform
                );
                currentTooltipPanel.name = CUSTOM_PANEL_NAME;
                currentTooltipPanel.SetActive(true);
                if(currentTooltipPanel.layer != 2) {
                    CustomLogger.Warning("{CustomTooltipLayerWarning}", tooltip.CustomTooltipPanel.name);
                }
            }
        }
    }

    public class Tooltip
    {
        public GameObject Target { get; private set; }
        public LString Text { get; private set; }
        public float? Width { get; private set; }
        public float? Height { get; private set; }
        public GameObject CustomTooltipPanel { get; private set; }

        /// <summary>
        /// Default tooltip, with a simple Panel with text
        /// </summary>
        public Tooltip(GameObject target, LString text, float? width = null, float? height = null)
        {
            Target = target;
            Text = text;
            Width = width;
            Height = height;
            CustomTooltipPanel = null;
        }

        /// <summary>
        /// Tooltip using a custom panel
        /// </summary>
        public Tooltip(GameObject target, GameObject customTooltipPanel)
        {
            Target = target;
            Text = null;
            Width = null;
            Height = null;
            CustomTooltipPanel = customTooltipPanel;
        }
    }
}
