//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();
            const double minDistance = 20.0; // Minimum distance between balls

            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition;
                bool validPosition;

                do
                {
                    validPosition = true;
                    startingPosition = new Vector(random.Next(100, 400 - 100), random.Next(100, 400 - 100));

                    foreach (Ball existingBall in BallsList)
                    {
                        double dx = existingBall.GetPosition().x - startingPosition.x;
                        double dy = existingBall.GetPosition().y - startingPosition.y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance < minDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                } while (!validPosition);

                Vector initialVelocity = new((random.NextDouble() - 1.5) * 2, (random.NextDouble() - 1.5) * 2); // Reduced velocity
                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          MoveTimer.Dispose();
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

        private void Move(object? x)
        {
            const double displayWidth = 400;  // Szerokość obszaru gry
            const double displayHeight = 400; // Wysokość obszaru gry
            const double ballRadius = 10;     // Promień kulki

            for (int i = 0; i < BallsList.Count; i++)
            {
                Ball ball = BallsList[i];

                // Oblicz nową pozycję kulki
                Vector newPosition = new Vector(ball.GetPosition().x + ball.Velocity.x, ball.GetPosition().y + ball.Velocity.y);

                // Sprawdź kolizję z lewą i prawą ścianą
                if (newPosition.x - ballRadius <= 0 || newPosition.x + ballRadius >= displayWidth)
                {
                    // Odwróć prędkość w osi X
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);

                    // Ustaw kulkę w granicach
                    double correctedX = Math.Clamp(newPosition.x, ballRadius, displayWidth - ballRadius);
                    ball.SetPosition(new Vector(correctedX, ball.GetPosition().y));
                }

                // Sprawdź kolizję z górną i dolną ścianą
                if (newPosition.y - ballRadius <= 0 || newPosition.y + ballRadius >= displayHeight)
                {
                    // Odwróć prędkość w osi Y
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);

                    // Ustaw kulkę w granicach
                    double correctedY = Math.Clamp(newPosition.y, ballRadius, displayHeight - ballRadius);
                    ball.SetPosition(new Vector(ball.GetPosition().x, correctedY));
                }

                // Przesuń kulkę
                ball.Move(ball.Velocity);
            }
        }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}