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
            const double minDistance = 15.0; // Minimum distance between balls

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

                Vector initialVelocity = new((random.NextDouble() - 1.5) * 2, (random.NextDouble() - 1.5) * 2);
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
            const double displayWidth = 400;  // Width of box
            const double displayHeight = 420; // Height of box
            const double ballRadius = 10;     // Ball radius

            for (int i = 0; i < BallsList.Count; i++)
            {
                Ball ball = BallsList[i];

                // New ball position
                Vector newPosition = new Vector(ball.GetPosition().x + ball.Velocity.x, ball.GetPosition().y + ball.Velocity.y);

                // Check collision with left and right walls
                if (newPosition.x - ballRadius <= -10 || newPosition.x + ballRadius >= displayWidth-14)
                {
                    // Invert X velocity
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);

                    // Place ball within bounds
                    double correctedX = Math.Clamp(newPosition.x, ballRadius, displayWidth - ballRadius);
                    ball.SetPosition(new Vector(correctedX, ball.GetPosition().y));
                }

                // Check collision with top and bottom walls
                if (newPosition.y - ballRadius <= -10 || newPosition.y + ballRadius >= displayHeight-14)
                {
                    // Invert Y velocity
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);

                    // Place ball within bounds
                    double correctedY = Math.Clamp(newPosition.y, ballRadius, displayHeight - ballRadius);
                    ball.SetPosition(new Vector(ball.GetPosition().x, correctedY));
                }

                // Check collision with other balls
                for (int j = i + 1; j < BallsList.Count; j++)
                {
                    Ball otherBall = BallsList[j];
                    Vector position1 = (Vector)ball.GetPosition();
                    Vector position2 = (Vector)otherBall.GetPosition();

                    double dx = position2.x - position1.x;
                    double dy = position2.y - position1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= 2 * ballRadius) // Collision detected
                    {
                        // Calculate new velocities using elastic collision formula
                        Vector velocity1 = (Vector)ball.Velocity;
                        Vector velocity2 = (Vector)otherBall.Velocity;

                        double mass = 1; // Assuming all balls have the same mass
                        double nx = dx / distance; // Normal vector
                        double ny = dy / distance;

                        double p = 2 * (velocity1.x * nx + velocity1.y * ny - velocity2.x * nx - velocity2.y * ny) / (2 * mass);

                        // Update velocities
                        ball.Velocity = new Vector(
                            velocity1.x - p * mass * nx,
                            velocity1.y - p * mass * ny
                        );

                        otherBall.Velocity = new Vector(
                            velocity2.x + p * mass * nx,
                            velocity2.y + p * mass * ny
                        );
                    }
                }

                // Move the ball
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