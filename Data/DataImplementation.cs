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

            // Initialize the diagnostic logging thread
            DiagnosticThread = new Thread(ProcessDiagnostics)
            {
                IsBackground = true
            };
            DiagnosticThread.Start();
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
                BallStartTimes[newBall] = DateTime.UtcNow;
                InitialVelocities[newBall] = initialVelocity;
                // Store normalized direction
                double speed = Math.Sqrt(initialVelocity.x * initialVelocity.x + initialVelocity.y * initialVelocity.y);
                BallDirections[newBall] = speed > 0 ? new Vector(initialVelocity.x / speed, initialVelocity.y / speed) : new Vector(1, 0);

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
                    Debug.WriteLine("Disposing DataImplementation...");

                    // Stop the diagnostic logging thread
                    DiagnosticQueue.CompleteAdding();
                    DiagnosticThread.Join();

                    foreach (var thread in BallThreads)
                    {
                        if (thread.IsAlive)
                        {
                            Debug.WriteLine($"Stopping thread {thread.ManagedThreadId}...");
                        }
                    }

                    MoveTimer.Dispose();
                    BallsList.Clear();
                    Debug.WriteLine("DataImplementation disposed.");
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

        private bool Disposed = false;

        private readonly Timer MoveTimer;
        private Random RandomGenerator = new();
        private readonly ConcurrentBag<Ball> BallsList = new(); // Concurrent collection of balls
        internal List<Thread> BallThreads = new(); // List of threads for each ball
        private readonly object LockObject = new(); // Lock object to prevent thread collisions
        private readonly ConcurrentDictionary<(Ball, Ball), bool> ActiveCollisions = new(); // Concurrent collision structure
        private readonly BlockingCollection<string> DiagnosticQueue = new(); // Diagnostic message queue
        private readonly Thread DiagnosticThread; // Thread for processing diagnostic messages
        private readonly string DiagnosticFilePath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName ?? AppDomain.CurrentDomain.BaseDirectory,
            "diagnostics.log"
        );
        private readonly object CollisionLock = new();
        private readonly ConcurrentDictionary<Ball, DateTime> BallStartTimes = new();
        private readonly ConcurrentDictionary<Ball, Vector> InitialVelocities = new();
        private readonly ConcurrentDictionary<Ball, Vector> BallDirections = new();

        private void Move(object? x)
        {
            const double displayWidth = 400;  // Width of the box
            const double displayHeight = 420; // Height of the box
            const double ballRadius = 10;     // Ball radius

            lock (LockObject) // Synchronize access to shared resources
            {
                foreach (var ball in BallsList)
                {
                    // Calculate the new position of the ball
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
            const double displayWidth = 400;
            const double displayHeight = 420;
            const double ballRadius = 10;

            // Get the initial velocity for direction and amplitude
            Vector initialVelocity = InitialVelocities.TryGetValue(ball, out var storedInitialVelocity)
                ? storedInitialVelocity
                : new Vector(1, 1);

            double initialSpeed = Math.Sqrt(initialVelocity.x * initialVelocity.x + initialVelocity.y * initialVelocity.y);

            // Get the start time for this ball
            DateTime startTime = BallStartTimes.TryGetValue(ball, out var t) ? t : DateTime.UtcNow;

            while (!Disposed)
            {
                // Get the current direction
                Vector direction = BallDirections.TryGetValue(ball, out var dir) ? dir : new Vector(1, 0);

                // Calculate elapsed time in seconds
                double elapsed = (DateTime.UtcNow - startTime).TotalSeconds;

                // Sinusoidal speed factor (oscillates between 0 and 1)
                double speedFactor = Math.Abs(Math.Sin(elapsed));

                // Set velocity as direction * (initial speed) * (sinusoidal factor)
                ball.Velocity = new Vector(
                    direction.x * initialSpeed * speedFactor,
                    direction.y * initialSpeed * speedFactor
                );

                // Calculate the new position of the ball
                Vector newPosition = new Vector(ball.GetPosition().x + ball.Velocity.x, ball.GetPosition().y + ball.Velocity.y);

                // Log diagnostic information
                LogDiagnostics($"Ball {ball.GetHashCode()} moved to position ({newPosition.x}, {newPosition.y}) with velocity ({ball.Velocity.x}, {ball.Velocity.y}), elapsed: {elapsed:F2}");

                // Collision logic (update direction on collision)
                lock (CollisionLock)
                {
                    foreach (var otherBall in BallsList)
                    {
                        if (ball == otherBall) continue;

                        Vector position1 = (Vector)ball.GetPosition();
                        Vector position2 = (Vector)otherBall.GetPosition();

                        double dx = position2.x - position1.x;
                        double dy = position2.y - position1.y;
                        double distance = Math.Sqrt(dx * dx + dy * dy);

                        if (distance <= 2 * ballRadius)
                        {
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

                            // Update direction for both balls after collision
                            double newSpeed1 = Math.Sqrt(ball.Velocity.x * ball.Velocity.x + ball.Velocity.y * ball.Velocity.y);
                            if (newSpeed1 > 0)
                                BallDirections[ball] = new Vector(ball.Velocity.x / newSpeed1, ball.Velocity.y / newSpeed1);

                            double newSpeed2 = Math.Sqrt(otherBall.Velocity.x * otherBall.Velocity.x + otherBall.Velocity.y * otherBall.Velocity.y);
                            if (newSpeed2 > 0)
                                BallDirections[otherBall] = new Vector(otherBall.Velocity.x / newSpeed2, otherBall.Velocity.y / newSpeed2);
                        }
                    }
                }

                // Wall collision logic (update direction on bounce)
                bool bounced = false;
                if (newPosition.x - ballRadius <= 0 || newPosition.x + ballRadius >= displayWidth - 20)
                {
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
                    double correctedX = Math.Clamp(newPosition.x, ballRadius, displayWidth - ballRadius);
                    ball.SetPosition(new Vector(correctedX, ball.GetPosition().y));
                    bounced = true;
                }
                if (newPosition.y - ballRadius <= 0 || newPosition.y + ballRadius >= displayHeight - 20)
                {
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
                    double correctedY = Math.Clamp(newPosition.y, ballRadius, displayHeight - ballRadius);
                    ball.SetPosition(new Vector(ball.GetPosition().x, correctedY));
                    bounced = true;
                }
                if (bounced)
                {
                    double newSpeed = Math.Sqrt(ball.Velocity.x * ball.Velocity.x + ball.Velocity.y * ball.Velocity.y);
                    if (newSpeed > 0)
                        BallDirections[ball] = new Vector(ball.Velocity.x / newSpeed, ball.Velocity.y / newSpeed);
                }
                else
                {
                    // Move the ball
                    ball.Move(ball.Velocity);
                }

                Thread.Sleep(10); // Control the speed of the ball
            }
        }

        private void LogDiagnostics(string message)
        {
            if (!Disposed && !DiagnosticQueue.IsAddingCompleted)
            {
                DiagnosticQueue.Add(message);
            }
        }

        private void ProcessDiagnostics()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(DiagnosticFilePath, append: true, encoding: System.Text.Encoding.ASCII))
                {
                    foreach (var message in DiagnosticQueue.GetConsumingEnumerable())
                    {
                        writer.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in diagnostic logging: {ex.Message}");
            }
        }

        #endregion TestingInfrastructure
    }
}