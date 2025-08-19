using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using netDxf;
using netDxf.Entities;
using DxfToCSharp.Core;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace DxfToCSharp
{
    public partial class MainWindow : Window
    {
        private TextEditor? _leftTextBox;
        private TextEditor? _rightTextBox;
        private TextBox? _errorsTextBox;
        private TabControl? _rightTabControl;
        private Border? _notificationBorder;
        private TextBlock? _notificationText;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _leftTextBox = this.FindControl<TextEditor>("LeftTextBox");
            _rightTextBox = this.FindControl<TextEditor>("RightTextBox");
            _errorsTextBox = this.FindControl<TextBox>("ErrorsTextBox");
            _rightTabControl = this.FindControl<TabControl>("RightTabControl");
            _notificationBorder = this.FindControl<Border>("NotificationBorder");
            _notificationText = this.FindControl<TextBlock>("NotificationText");
            
            InitializeTextMate();
        }
        
        private void InitializeTextMate()
        {
            if (_rightTextBox != null)
            {
                var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
                var textMateInstallation = _rightTextBox.InstallTextMate(registryOptions);
                textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".cs").Id));
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnOpenClicked(object? sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Open DXF",
                AllowMultiple = false,
                Filters =
                {
                    new FileDialogFilter { Name = "DXF files", Extensions = { "dxf" } },
                    new FileDialogFilter { Name = "All files", Extensions = { "*" } }
                }
            };
            var result = await ofd.ShowAsync(this);
            var path = result?.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                return;

            // Load text to left panel
            string text = await System.IO.File.ReadAllTextAsync(path);
            if (_leftTextBox != null)
                _leftTextBox.Text = text;

            // Parse with netDxf and generate C# code
            try
            {
                var doc = DxfDocument.Load(path);
                if (doc == null)
                {
                    ShowError("Failed to load DXF document.\n");
                    SetRightText(""); // Clear generated code editor
                    ShowNotification("Failed to load DXF document", true);
                    return;
                }

                var generator = new DxfCodeGenerator();
                var code = generator.Generate(doc, path);
                SetRightText(code);
                ClearErrors();
                
                // Check if there are no entities to warn the user
                var allEntities = doc.Entities.All?.ToList() ?? new List<EntityObject>();
                if (allEntities.Count == 0)
                {
                    ShowNotification("Warning: No entities found in DXF file to generate code for", true);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error: " + ex.Message + "\n" + ex.StackTrace);
                SetRightText(""); // Clear generated code editor
                ShowNotification("Failed to load DXF file: " + ex.Message, true);
            }
        }

        private async void OnRunClicked(object? sender, RoutedEventArgs e)
        {
            try
            {
                ClearErrors();
                var code = _rightTextBox?.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    ShowError("No generated code to compile.");
                    return;
                }
                var scripting = new DxfToCSharp.Services.CSharpScriptingService();
                var comp = scripting.Compile(code);
                if (!comp.Success || string.IsNullOrEmpty(comp.AssemblyPath))
                {
                    ShowError("Compilation failed:\n" + comp.Output);
                    ShowNotification("Compilation failed. Check errors tab for details.", isError: true);
                    return;
                }
                var output = scripting.RunCreateMethod(comp.AssemblyPath);
                if (output.StartsWith("Runtime error:") || output.StartsWith("Type '") || output.StartsWith("Static method"))
                {
                    ShowError(output);
                    ShowNotification("Execution failed. Check errors tab for details.", isError: true);
                    return;
                }
                // Show success notification instead of putting result in code control
                if (output.StartsWith("Executed successfully"))
                {
                    ShowNotification("DXF file generated successfully!", isError: false);
                }
                else
                {
                    ShowNotification(output, isError: false);
                }
            }
            catch (Exception ex)
            {
                ShowError("Execution error:\n" + ex);
                ShowNotification("Execution error occurred. Check errors tab for details.", isError: true);
            }
        }

        private void SetLeftText(string text)
        {
            if (_leftTextBox != null)
                _leftTextBox.Text = text;
        }

        private void SetRightText(string text)
        {
            if (_rightTextBox != null)
                _rightTextBox.Text = text;
            if (_rightTabControl != null)
                _rightTabControl.SelectedIndex = 0; // Code tab
        }

        private void ShowError(string message)
        {
            if (_errorsTextBox != null)
                _errorsTextBox.Text = message;
            if (_rightTabControl != null)
                _rightTabControl.SelectedIndex = 1; // Errors tab
        }

        private void ClearErrors()
        {
            if (_errorsTextBox != null)
                _errorsTextBox.Text = string.Empty;
        }

        private void ShowNotification(string message, bool isError = false)
        {
            if (_notificationText != null && _notificationBorder != null)
            {
                _notificationText.Text = message;
                _notificationBorder.Background = isError ? 
                    Avalonia.Media.Brushes.Red : 
                    Avalonia.Media.Brushes.Green;
                _notificationBorder.IsVisible = true;
                
                // Auto-hide after 5 seconds
                Task.Delay(5000).ContinueWith(_ => 
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        if (_notificationBorder != null)
                            _notificationBorder.IsVisible = false;
                    });
                });
            }
        }

        private void OnCloseNotificationClicked(object? sender, RoutedEventArgs e)
        {
            if (_notificationBorder != null)
                _notificationBorder.IsVisible = false;
        }
    }
}