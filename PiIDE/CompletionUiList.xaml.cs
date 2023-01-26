﻿using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System;

namespace PiIDE {

    public partial class CompletionUiList : UserControl {

        private readonly string FilePath;
        public EventHandler<Completion>? CompletionClicked;

        public CompletionUiList(string filePath) {
            InitializeComponent();
            FilePath = filePath;
            Close();
        }

        public Completion SelectedCompletion => (Completion) MainListBox.SelectedItem;
        public int CompletionsCount => MainListBox.Items.Count;
        public bool SelectedAnIndex => MainListBox.SelectedIndex >= 0;
        public bool IsOpen => MainListBox.IsVisible;

        private void AddCompletions(Completion[] completions) {
            MainListBox.ItemsSource = completions;
            MainListBox.Visibility = Visibility.Visible;
        }

        private void ClearCompletions() {
            MainListBox.ItemsSource = null;
        }

        public async Task ReloadCompletionsAsync(string fileContent, int caretLine, int caretColumn) {
            ClearCompletions();
            Completion[] completions = await JediCompletionWraper.GetCompletionAsync(FilePath, fileContent, caretLine, caretColumn);

            if (completions.Length == 0) {
                Close();
                return;
            }

            AddCompletions(completions);
        }

        public void Close() {
            ClearCompletions();
            MainListBox.Visibility = Visibility.Collapsed;
        }

        public void MoveSelectedCompletionUp() {

            if (MainListBox.Items.Count == 0)
                return;

            if (MainListBox.SelectedIndex == 0)
                MainListBox.SelectedIndex = CompletionsCount - 1;
            
            --MainListBox.SelectedIndex;

            MainListBox.ScrollIntoView(SelectedCompletion);
        }

        public void MoveSelectedCompletionDown() {

            if (MainListBox.Items.Count == 0)
                return;

            if (MainListBox.SelectedIndex == CompletionsCount - 1)
                MainListBox.SelectedIndex = 0;
            
            ++MainListBox.SelectedIndex;

            MainListBox.ScrollIntoView(SelectedCompletion);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            // TODO: make this work:
            //CompletionClicked?.Invoke(sender, (Completion) MainListBox.Items[index]);
            Close();
        }
    }
}
