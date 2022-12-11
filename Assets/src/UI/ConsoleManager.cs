using Game.Input;
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
        private static readonly string VARIABLE_PREFIX = "$";

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
        private Dictionary<string, object> variables = new Dictionary<string, object>();

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
            Text.text = string.Empty;
            Text.faceColor = new Color(1.0f, 1.0f, 1.0f);
            Active = false;
            Tags.Add(Tag.Console);

            //Create commands
            commands.Add(new Command("echo", null, "Print text to console", (List<string> parameters) => {
                if(parameters.Count == 0) {
                    return null;
                }
                string output = string.Join(" ", parameters);
                foreach(KeyValuePair<string, object> variable in variables) {
                    //TODO: variable name endings
                    //Example:
                    //test1 = "abc"
                    //test12 = "def"
                    //echo $test12
                    //prints: abc2
                    output = output.Replace(VARIABLE_PREFIX + variable.Key, variable.Value.ToString());
                }
                return output;
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

            commands.Add(new Command("setVariable", new List<string>() { "set_variable", "setVar", "set_var" }, "Create a new variable or assing a new value to a pre-existing variable", (List<string> parameters) => {
                return SetVariable(parameters);
            }));

            commands.Add(new Command("deleteVariable", new List<string>() { "delete_variable", "delVar", "del_var" }, "Delete a variable (if it exists)", (List<string> parameters) => {
                if (parameters.Count != 1) {
                    return new LString("ConsoleCommandInvalidArgumentCount", LTables.Game, 1);
                }
                if (!variables.ContainsKey(parameters[0])) {
                    return "{ConsoleVariableDoesNotExist}";
                }
                variables.Remove(parameters[0]);
                return "{ConsoleVariableDeleted}";
            }));

            commands.Add(new Command("listVariables", new List<string>() { "list_variables", "listVars", "list_vars" }, "Prints list of all the current variables, along with their values", (List<string> parameters) => {
                if(variables.Count == 0) {
                    return "{ConsoleNoVariables}";
                }
                StringBuilder output = new StringBuilder();
                for (int i = 0; i < variables.Count; i++) {
                    output.Append(GetVariablePrint(variables.ElementAt(i).Key));
                    if (i != variables.Count - 1) {
                        output.Append(Environment.NewLine);
                    }
                }
                return output.ToString();
            }));

            commands.Add(new Command("startDiagnostics", new List<string>() { "start_diagnostics" }, "Start clocking diagnostics. Can be run without parameters, or given a list of tags to clock.", (List<string> parameters) => {
                bool wasRunning = DiagnosticsManager.IsRunning;
                if(parameters.Count == 0) {
                    DiagnosticsManager.Start();
                } else {
                    List<DiagnosticsManager.Tag> tags = new List<DiagnosticsManager.Tag>();
                    foreach(string parameter in parameters) {
                        int intParameter;
                        if(int.TryParse(parameter, out intParameter)) {
                            tags.Add((DiagnosticsManager.Tag)intParameter);
                        } else {
                            DiagnosticsManager.Tag? parsedTag = null;
                            foreach (DiagnosticsManager.Tag tag in Enum.GetValues(typeof(DiagnosticsManager.Tag))) {
                                if(tag.ToString() == parameter) {
                                    parsedTag = tag;
                                    break;
                                }
                            }
                            if (parsedTag.HasValue) {
                                tags.Add(parsedTag.Value);
                            } else {
                                return new LString("InvalidDiagnosticsTag", LTables.Log, parameter);
                            }
                        }
                    }
                    DiagnosticsManager.Start(tags);
                }
                return new LString(wasRunning ? "RestartedClockingDiagnostics" : "StartedClockingDiagnostics", LTables.Log);
            }));

            commands.Add(new Command("endDiagnostics", new List<string>() { "end_diagnostics" }, "End diagnostics clocking and print results", (List<string> parameters) => {
                Dictionary<string, long> totals = DiagnosticsManager.End();
                StringBuilder output = new StringBuilder();
                output.AppendLine(new LString("DiagnosticsResults", LTables.Log));
                if(totals.Count != 0) {
                    for (int i = 0; i < totals.Count; i++) {
                        output.Append(totals.ElementAt(i).Key).Append(": ").Append(totals.ElementAt(i).Value).Append("ms");
                        if (i != totals.Count - 1) {
                            output.Append(Environment.NewLine);
                        }
                    }
                } else {
                    output.Append(new LString("NoResults", LTables.Log));
                }
                return output.ToString();
            }));

            commands.Add(new Command("debugWindow", new List<string>() { "debug_window", "dw" }, "Opens and closes debug window", (List<string> parameters) => {
                DebugWindowManager.Instance.Active = !DebugWindowManager.Instance.Active;
                return string.Format("Debug window {0}", DebugWindowManager.Instance.Active ? "opened" : "closed");
            }));

            commands.Add(new Command("debugMouse", new List<string>() { "mouse_debug_log", "debug_mouse" }, "Toggles MouseManager debug log level", (List<string> parameters) => {
                MouseManager.Instance.DebugLogLevel = MouseManager.Instance.DebugLogLevel.Shift(1);
                return string.Format("Mouse debug log: {0}", MouseManager.Instance.DebugLogLevel);
            }));

            commands.Add(new Command("clearEffects", new List<string>() { "clear_effects", "ce" }, "Clears all effects", (List<string> parameters) => {
                int count = Effect2DManager.Instance.ActiveEffectCount;
                Effect2DManager.Instance.RemoveAll();
                return string.Format("{0} effect(s) cleared", count);
            }));

            commands.Add(new Command("mouseEventListenerCounts", new List<string>() { }, "Shows current number of mouse event listeners registered", (List<string> parameters) => {
                StringBuilder output = new StringBuilder();
                output.AppendLine("Mouse event listener counts");
                output.AppendLine(string.Format("Object click: {0}", MouseManager.Instance.ClickListenerCount));
                output.AppendLine(string.Format("Nothing click: {0}", MouseManager.Instance.NothingClickListenerCount));
                output.AppendLine(string.Format("Drag: {0}", MouseManager.Instance.DragListenerCount));
                output.Append(string.Format("Total: {0}", MouseManager.Instance.TotalListenerCount));
                return output.ToString();
            }));

            commands.Add(new Command("backToMenu", new List<string>() { }, "Returns game state back to main menu", (List<string> parameters) => {
                if(Main.Instance.State == State.Running) {
                    Main.Instance.BackToMainMenu();
                    return "Game state set to State.MainMenu";
                }
                return Main.Instance.State == State.MainMenu ? "Already in main menu" : "Can't return to main menu when game state is " + Main.Instance.State;
            }));

            //----- TEMPLATE PROJECT DEBUGGING COMMANDS -----
            commands.Add(new Command("togglePlayerMovement", null, "Changes players movement type (DELETE THIS: TEMPLATE PROJECT DEBUGGING ONLY)", (List<string> parameters) => {
                Main.Instance.PlayerCharacter.Movement = Main.Instance.PlayerCharacter.Movement.Shift(1);
                return Main.Instance.PlayerCharacter.Movement.ToString();
            }));

            commands.Add(new Command("test1", null, "Test command 1", (List<string> parameters) => {
                return "Test command 1";
            }));

            commands.Add(new Command("test2", null, "Test command 2", (List<string> parameters) => {
                return "Test command 2";
            }));

            //Check for duplicate names
            if (commands.Select(command => command.Name).Distinct().Count() != commands.Count) {
                CustomLogger.Warning("{DuplicatedConsoleCommands}");
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
                //Command found
                //Remove name from parameter list
                parameters.RemoveAt(0);
                //Assing variables
                parameters = parameters.Select(parameter => {
                    if (parameter.StartsWith(VARIABLE_PREFIX) && parameter.Length > 1 && variables.ContainsKey(parameter.Substring(1))) {
                        return variables[parameter.Substring(1)].ToString();
                    }
                    return parameter;
                }).ToList();
                //Run command
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

        public void AutoComplete()
        {
            if (!Active || string.IsNullOrEmpty(Input.text)) {
                return;
            }
            foreach(Command command in commands) {
                if (command.Name.StartsWith(Input.text)) {
                    Input.text = command.Name;
                    return;
                }
            }
            foreach (Command command in commands) {
                foreach(string alias in command.Aliases) {
                    if (alias.StartsWith(Input.text)) {
                        Input.text = alias;
                        return;
                    }
                }
            }
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

        private string SetVariable(string name, string value)
        {
            return SetVariable(new List<string>() { name, value });
        }

        private string SetVariable(List<string> parameters)
        {
            if (parameters.Count != 2) {
                //TODO: Add support for arrays
                return new LString("ConsoleCommandInvalidArgumentCount", LTables.Game, 2);
            }
            string variableName = parameters[0];
            string variableValueS = parameters[1];
            object variableValueO = null;
            //Check if value is int
            int intValue;
            if (int.TryParse(variableValueS, out intValue)) {
                variableValueO = intValue;
            }

            //Check if value is long
            if(variableValueO == null) {
                long longValue;
                if (long.TryParse(variableValueS, out longValue)) {
                    variableValueO = longValue;
                }
            }

            if (variableValueO == null) {
                //Variable is string
                variableValueO = variableValueS;
            }

            //Add or update
            if (variables.ContainsKey(variableName)) {
                variables[variableName] = variableValueO;
            } else {
                variables.Add(variableName, variableValueO);
            }

            string quotes = variableValueO.GetType() == typeof(string) ? "\"" : string.Empty;
            return GetVariablePrint(variableName);
        }

        private string GetVariablePrint(string name)
        {
            if (!variables.ContainsKey(name)) {
                return string.Empty;
            }
            object variableValue = variables[name];
            string quotes = variableValue.GetType() == typeof(string) ? "\"" : string.Empty;
            return string.Format("{0} = {1}{2}{3}", name, quotes, variableValue.ToString(), quotes);
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
            public delegate LString ActionDelegate(List<string> parameters);

            public string Name { get;private set; }
            public List<string> Aliases { get; private set; }
            /// <summary>
            /// Description split into lines
            /// </summary>
            public List<LString> Description { get; private set; }
            public ActionDelegate Action { get; private set; }
            public bool SaveToHistory { get; private set; }

            public Command(string name, List<string> aliases, List<LString> description, ActionDelegate action, bool saveToHistory = true)
            {
                if (string.IsNullOrEmpty(name) || action == null) {
                    throw new ArgumentNullException();
                }
                Name = name;
                Aliases = aliases == null ? new List<string>() : aliases.Copy();
                Description = description == null ? new List<LString>() : description.Copy();
                Action = action;
                SaveToHistory = saveToHistory;
            }

            public Command(string name, List<string> aliases, LString description, ActionDelegate action, bool saveToHistory = true)
            {
                if(string.IsNullOrEmpty(name) || action == null) {
                    throw new ArgumentNullException();
                }
                Name = name;
                Aliases = aliases == null ? new List<string>() : aliases.Copy();
                Description = new List<LString>() { description };
                Action = action;
                SaveToHistory = saveToHistory;
            }
        }
    }
}
