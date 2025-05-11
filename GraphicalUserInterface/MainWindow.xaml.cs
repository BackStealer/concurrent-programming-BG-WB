//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    /// <summary>
    /// View implementation
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Prompt the user for the number of balls
            int numberOfBalls = GetNumberOfBallsFromUser();

            // Pass the number of balls to the ViewModel
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            viewModel.Start(numberOfBalls);
        }

        private int GetNumberOfBallsFromUser()
        {
            while (true)
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter the number of balls:",
                    "Ball Simulation",
                    "10" // Default value
                );

                if (int.TryParse(input, out int numberOfBalls) && numberOfBalls > 0)
                {
                    return numberOfBalls;
                }

                MessageBox.Show("Invalid input. Please enter a positive integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Raises the <seealso cref="System.Windows.Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}