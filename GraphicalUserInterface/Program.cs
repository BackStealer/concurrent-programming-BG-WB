using System;
using System.Windows;
using TP.ConcurrentProgramming.Data;
using TP.ConcurrentProgramming.Presentation.ViewModel;
using TP.ConcurrentProgramming.PresentationView;

namespace TP.ConcurrentProgramming.ConsoleApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            int numberOfBalls = GetNumberOfBallsFromUser();
            DataAbstractAPI dataLayer = DataAbstractAPI.GetDataLayer();
            dataLayer.Start(numberOfBalls, (position, ball) =>
            {
                // Handle the ball initialization in the upper layer
                Console.WriteLine($"Ball initialized at position: ({position.x}, {position.y})");
            });

            // Start the WPF application
            var app = new Application();
            var mainWindow = new MainWindow();
            app.Run(mainWindow);
        }

        static int GetNumberOfBallsFromUser()
        {
            Console.Write("Enter the number of balls: ");
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int numberOfBalls) && numberOfBalls > 0)
                {
                    return numberOfBalls;
                }
                Console.Write("Invalid input. Please enter a positive integer: ");
            }
        }
    }
}
