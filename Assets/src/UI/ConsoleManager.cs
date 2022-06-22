using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class ConsoleManager : WindowBase
    {
        public static ConsoleManager Instance;

        /// <summary>
        /// Is console enabled for the player? (Game can still use console internally, even if this is false)
        /// </summary>
        public bool Enabled { get; set; } = false;

        public GameObject ScrollViewContent;
        public GameObject ScrollBar;
        public TMP_Text Text;
        public TMP_InputField Input;

        private List<Command> commands = new List<Command>();
        private List<ArchivedCommand> history = new List<ArchivedCommand>();
        private int? historyPosition = null;
        private bool delayedScrollDown = false;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            Instance = this;
            Text.text = string.Empty;
            Active = false;
            Tags.Add(Tag.Console);

            //Create commands
            commands.Add(new Command("echo", null, "Print text to console", (List<string> parameters) => {
                if(parameters.Count == 0) {
                    return null;
                }
                return string.Join(" ", parameters);
            }));

            commands.Add(new Command("list", new List<string>() { "li" }, "Shows this list :)", (List<string> parameters) => {
                StringBuilder output = new StringBuilder();
                for(int i = 0; i < commands.Count; i++) {
                    Command command = commands[i];
                    output.Append(" - ").Append(command.Name);
                    if(command.Aliases.Count != 0) {
                        output.Append(" (").Append(string.Join(", ", command.Aliases)).Append(")");
                    }
                    foreach (string line in command.Description) {
                        output.Append(Environment.NewLine).Append("   ").Append(line);
                    }
                    if(i != commands.Count - 1) {
                        output.Append(Environment.NewLine);
                    }
                }
                return output.ToString();
            }));

            commands.Add(new Command("clearScreen", new List<string>() { "clear_screen", "cls" }, "Clear all text from console screen (command history is not affected)", (List<string> parameters) => {
                foreach(ArchivedCommand archivedCommand in history) {
                    archivedCommand.Visible = false;
                }
                Text.text = string.Empty;
                return null;
            }));

            commands.Add(new Command("clearHistory", new List<string>() { "clear_history", "clh" }, "Clear all text from console screen along with command history", (List<string> parameters) => {
                history.Clear();
                Text.text = string.Empty;
                return null;
            }, false));

            //Check for duplicate names
            if (commands.Select(command => command.Name).Distinct().Count() != commands.Count) {
                CustomLogger.Warning("DuplicatedConsoleCommands");
            }

            //Sort
            commands = commands.OrderBy(command => command.Name).ToList();
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            if (delayedScrollDown && ScrollBar.GetComponent<Scrollbar>().value != 0.0f) {
                //Some times scrollbar position does not update to properly after command is run in RunCommand
                //This is here to make sure it scrolls to the bottom after command is ran
                ScrollBar.GetComponent<Scrollbar>().value = 0.0f;
                delayedScrollDown = false;
            }
        }

        public override bool HandleWindowEvent(WindowEvent windowEvent)
        {
            if (!Enabled) {
                return false;
            }
            switch (windowEvent) {
                case WindowEvent.Close:
                    Active = false;
                    break;
                case WindowEvent.Accept:
                    RunCommand();
                    break;
            }
            return true;
        }

        public override bool Active
        {
            get => base.Active;
            set {
                if(value && !Enabled) {
                    return;
                }
                base.Active = value;
                if (value) {
                    FocusInput();
                }
            }
        }

        /// <summary>
        /// Run a console command (note: works even if console is closed)
        /// </summary>
        /// <param name="commandParam">Command to be run. If this parameter is not provided, console reads command from input field.</param>
        /// <param name="internalCall">If true, command does not show up in command history.</param>
        public void RunCommand(string commandParam = null, bool internalCall = false)
        {
            string commandCall = string.IsNullOrEmpty(commandParam) ? Input.text : commandParam;
            if (string.IsNullOrEmpty(commandParam)) {
                Input.text = string.Empty;
            }
            commandCall = commandCall.Trim();
            List<string> parameters = commandCall.Split(" ").ToList();
            if(parameters.Count == 0) {
                //Empty call
                return;
            }

            //Find by command name
            Command command = commands.FirstOrDefault(command => command.Name == parameters[0]);
            if(command == null) {
                //Find by alias
                command = commands.FirstOrDefault(command => command.Aliases.Contains(parameters[0]));
            }

            string output;
            if (command == null) {
                //Command not found
                output = string.Format(Localization.Log.Get("CommandNotRecognized"), commandCall);
                history.Add(new ArchivedCommand() {
                    Input = commandCall,
                    Output = output,
                    Visible = true,
                    InternalCall = internalCall,
                    Command = null
                });
                Text.text = Text.text + output + Environment.NewLine;
            } else {
                //Run command
                parameters.RemoveAt(0);
                output = command.Action(parameters);
                if (command.SaveToHistory) {
                    history.Add(new ArchivedCommand() {
                        Input = commandCall,
                        Output = output,
                        Visible = true,
                        InternalCall = internalCall,
                        Command = command
                    });
                }
                if (output != null) {
                    //Add to screen
                    Text.text = Text.text + output + Environment.NewLine;
                }
            }

            historyPosition = null;
            ScrollViewContent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Text.preferredHeight + 10.0f);
            ScrollBar.GetComponent<Scrollbar>().value = 0.0f;
            delayedScrollDown = true;
            
            FocusInput();
        }

        public void HistoryUp()
        {
            MoveHistoryPosition(true);
        }

        public void HistoryDown()
        {
            MoveHistoryPosition(false);
        }

        private void MoveHistoryPosition(bool up)
        {
            if (!Active) {
                return;
            }
            List<ArchivedCommand> accessableHistory = history.Where(command => !command.InternalCall).ToList();
            if (accessableHistory.Count == 0) {
                return;
            }
            if (historyPosition.HasValue) {
                if (up && historyPosition.Value != 0) {
                    historyPosition--;
                } else if (!up && historyPosition.Value != accessableHistory.Count - 1) {
                    historyPosition++;
                }
            } else {
                historyPosition = accessableHistory.Count - 1;
            }
            Input.text = accessableHistory[historyPosition.Value].Input;
        }

        private void FocusInput()
        {
            EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
            Input.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private class ArchivedCommand
        {
            public string Input { get; set; }
            public string Output { get; set; }
            public bool Visible { get; set; }
            public bool InternalCall { get; set; }
            public Command Command { get; set; }
        }

        private class Command
        {
            public delegate string ActionDelegate(List<string> parameters);

            public string Name { get;private set; }
            public List<string> Aliases { get; private set; }
            /// <summary>
            /// Description split into lines
            /// </summary>
            public List<string> Description { get; private set; }
            public ActionDelegate Action { get; private set; }
            public bool SaveToHistory { get; private set; }

            public Command(string name, List<string> aliases, List<string> description, ActionDelegate action, bool saveToHistory = true)
            {
                if (string.IsNullOrEmpty(name) || action == null) {
                    throw new ArgumentNullException();
                }
                Name = name;
                Aliases = aliases == null ? new List<string>() : aliases.Copy();
                Description = description == null ? new List<string>() : description.Copy();
                Action = action;
                SaveToHistory = saveToHistory;
            }

            public Command(string name, List<string> aliases, string description, ActionDelegate action, bool saveToHistory = true)
            {
                if(string.IsNullOrEmpty(name) || action == null) {
                    throw new ArgumentNullException();
                }
                Name = name;
                Aliases = aliases == null ? new List<string>() : aliases.Copy();
                Description = new List<string>() { description };
                Action = action;
                SaveToHistory = saveToHistory;
            }
        }
    }
}
