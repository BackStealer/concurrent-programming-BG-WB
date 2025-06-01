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
using System.Collections.Concurrent;

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

                Vector initialVelocity = new((random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2);
                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);

                Thread ballThread = new Thread(() => MoveBall(newBall))
                {
                    IsBackground = true
                };
                BallThreads.Add(ballThread);
                ballThread.Start();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    System.Diagnostics.Debug.WriteLine("Disposing DataImplementation...");

                    foreach (var thread in BallThreads)
                    {
                        if (thread.IsAlive)
                        {
                            System.Diagnostics.Debug.WriteLine($"Stopping thread {thread.ManagedThreadId}...");
                        }
                    }

                    MoveTimer.Dispose();
                    BallsList.Clear();
                    System.Diagnostics.Debug.WriteLine("DataImplementation disposed.");
                }
                Disposed = true;
            }
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
        private readonly ConcurrentBag<Ball> BallsList = new(); // Współbieżna kolekcja piłek
        internal List<Thread> BallThreads = new(); // List of threads for each ball
        private readonly object LockObject = new(); // Lock object to prevent thread collisions
        private readonly ConcurrentDictionary<(Ball, Ball), bool> ActiveCollisions = new(); // Współbieżna struktura kolizji

        private void Move(object? x)
        {
            const double displayWidth = 400;  // Width of box
            const double displayHeight = 420; // Height of box
            const double ballRadius = 10;     // Ball radius

            lock (LockObject) // Synchronize access to shared resources
            {
                foreach (var ball in BallsList)
                {
                    // New ball position
                    Vector newPosition = new Vector(ball.GetPosition().x + ball.Velocity.x, ball.GetPosition().y + ball.Velocity.y);

                    // Check collision with left and right walls
                    if (newPosition.x - ballRadius <= 0 || newPosition.x + ballRadius >= displayWidth - 20)
                    {
                        ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
                        double correctedX = Math.Clamp(newPosition.x, ballRadius, displayWidth - ballRadius);
                        ball.SetPosition(new Vector(correctedX, ball.GetPosition().y));
                    }

                    // Check collision with top and bottom walls
                    if (newPosition.y - ballRadius <= 0 || newPosition.y + ballRadius >= displayHeight - 20)
                    {
                        ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
                        double correctedY = Math.Clamp(newPosition.y, ballRadius, displayHeight - ballRadius);
                        ball.SetPosition(new Vector(ball.GetPosition().x, correctedY));
                    }

                    // Check collision with other balls
                    foreach (var otherBall in BallsList)
                    {
                        if (ball == otherBall) continue;

                        Vector position1 = (Vector)ball.GetPosition();
                        Vector position2 = (Vector)otherBall.GetPosition();

                        double dx = position2.x - position1.x;
                        double dy = position2.y - position1.y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= 2 * ballRadius) // Collision detected
                        {
                            Vector velocity1 = (Vector)ball.Velocity;
                            Vector velocity2 = (Vector)otherBall.Velocity;

                            double nx = dx / distance;
                            double ny = dy / distance;

                            double p = (velocity1.x * nx + velocity1.y * ny - velocity2.x * nx - velocity2.y * ny);

                            ball.Velocity = new Vector(
                                velocity1.x - p * nx,
                                velocity1.y - p * ny
                            );

                            otherBall.Velocity = new Vector(
                                velocity2.x + p * nx,
                                velocity2.y + p * ny
                            );
                        }
                    }

                    // Move the ball
                    ball.Move(ball.Velocity);
                }
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
      returnNumberOfBalls(BallsList.Count());
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

        private void MoveBall(Ball ball)
        {
            const double displayWidth = 400;  // Width of box
            const double displayHeight = 420; // Height of box
            const double ballRadius = 10;     // Ball radius

            while (!Disposed)
            {
                // Oblicz nową pozycję piłki
                Vector newPosition = new Vector(ball.GetPosition().x + ball.Velocity.x, ball.GetPosition().y + ball.Velocity.y);

                // Sprawdź kolizje ze ścianami
                if (newPosition.x - ballRadius <= 0 || newPosition.x + ballRadius >= displayWidth - 14)
                {
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
                    double correctedX = Math.Clamp(newPosition.x, ballRadius, displayWidth - ballRadius);
                    ball.SetPosition(new Vector(correctedX, ball.GetPosition().y));
                }

                if (newPosition.y - ballRadius <= 0 || newPosition.y + ballRadius >= displayHeight - 14)
                {
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
                    double correctedY = Math.Clamp(newPosition.y, ballRadius, displayHeight - ballRadius);
                    ball.SetPosition(new Vector(ball.GetPosition().x, correctedY));
                }

                // Sprawdź kolizje z innymi piłkami
                foreach (Ball otherBall in BallsList)
                {
                    if (ball == otherBall) continue;

                    Vector position1 = (Vector)ball.GetPosition();
                    Vector position2 = (Vector)otherBall.GetPosition();

                    double dx = position2.x - position1.x;
                    double dy = position2.y - position1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    if (distance <= 2 * ballRadius) // Kolizja wykryta
                    {
                        var collisionPair = (ball, otherBall);

                        // Dodaj kolizję do ActiveCollisions, jeśli jeszcze jej nie ma
                        if (!ActiveCollisions.TryAdd(collisionPair, true))
                            continue;

                        // Oblicz nowe prędkości
                        Vector velocity1 = (Vector)ball.Velocity;
                        Vector velocity2 = (Vector)otherBall.Velocity;

                        double nx = dx / distance;
                        double ny = dy / distance;

                        double p = 2 * (velocity1.x * nx + velocity1.y * ny - velocity2.x * nx - velocity2.y * ny) / 2;

                        ball.Velocity = new Vector(
                            velocity1.x - p * nx,
                            velocity1.y - p * ny
                        );

                        otherBall.Velocity = new Vector(
                            velocity2.x + p * nx,
                            velocity2.y + p * ny
                        );

                        // Usuń kolizję z ActiveCollisions
                        ActiveCollisions.TryRemove(collisionPair, out _);
                    }
                }

                // Przesuń piłkę
                ball.Move(ball.Velocity);

                Thread.Sleep(10); // Kontrola prędkości piłki
            }
        }

    #endregion TestingInfrastructure
  }
}