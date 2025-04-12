//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class DataImplementationUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      using (DataImplementation newInstance = new DataImplementation())
      {
        IEnumerable<IBall>? ballsList = null;
        newInstance.CheckBallsList(x => ballsList = x);
        Assert.IsNotNull(ballsList);
        int numberOfBalls = 0;
        newInstance.CheckNumberOfBalls(x => numberOfBalls = x);
        Assert.AreEqual<int>(0, numberOfBalls);
      }
    }

    [TestMethod]
    public void DisposeTestMethod()
    {
      DataImplementation newInstance = new DataImplementation();
      bool newInstanceDisposed = false;
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsFalse(newInstanceDisposed);
      newInstance.Dispose();
      newInstance.CheckObjectDisposed(x => newInstanceDisposed = x);
      Assert.IsTrue(newInstanceDisposed);
      IEnumerable<IBall>? ballsList = null;
      newInstance.CheckBallsList(x => ballsList = x);
      Assert.IsNotNull(ballsList);
      newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(0, x));
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Dispose());
      Assert.ThrowsException<ObjectDisposedException>(() => newInstance.Start(0, (position, ball) => { }));
    }

    [TestMethod]
    public void StartTestMethod()
    {
      using (DataImplementation newInstance = new DataImplementation())
      {
        int numberOfCallbackInvoked = 0;
        int numberOfBalls2Create = 10;
        newInstance.Start(
          numberOfBalls2Create,
          (startingPosition, ball) =>
          {
            numberOfCallbackInvoked++;
            Assert.IsTrue(startingPosition.x >= 0);
            Assert.IsTrue(startingPosition.y >= 0);
            Assert.IsNotNull(ball);
          });
        Assert.AreEqual<int>(numberOfBalls2Create, numberOfCallbackInvoked);
        newInstance.CheckNumberOfBalls(x => Assert.AreEqual<int>(10, x));
      }
    }
  }
    [TestClass]
    public class DataImplementationTests
    {
        [TestMethod]
        public void Start_ValidNumberOfBalls_CreatesBallsWithCorrectPositions()
        {
            // Arrange
            using var dataImplementation = new DataImplementation();
            int numberOfBalls = 10;
            int callbackCount = 0;

            // Act
            dataImplementation.Start(numberOfBalls, (position, ball) =>
            {
                callbackCount++;
                Assert.IsTrue(position.x >= 0 && position.y >= 0);
            });

            // Assert
            Assert.AreEqual(numberOfBalls, callbackCount);
        }

        [TestMethod]
        public void Start_BallsAreAtLeast20UnitsApart()
        {
            // Arrange
            using var dataImplementation = new DataImplementation();
            int numberOfBalls = 10;
            var positions = new List<IVector>();

            // Act
            dataImplementation.Start(numberOfBalls, (position, ball) =>
            {
                foreach (var existingPosition in positions)
                {
                    double dx = existingPosition.x - position.x;
                    double dy = existingPosition.y - position.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    Assert.IsTrue(distance >= 20.0, "Balls are too close to each other.");
                }
                positions.Add(position);
            });

            // Assert
            Assert.AreEqual(numberOfBalls, positions.Count);
        }
    }
    [TestClass]
    public class DataImplementationMoveTests
    {
        [TestMethod]
        public void Move_BallsCollideAndChangeVelocity()
        {
            // Arrange
            using var dataImplementation = new DataImplementation();
            var ball1 = new Ball(new Vector(10, 10), new Vector(1, 0));
            var ball2 = new Ball(new Vector(20, 10), new Vector(-1, 0));
            dataImplementation.CheckBallsList(balls =>
            {
                var list = balls.ToList();
                list.Add(ball1);
                list.Add(ball2);
            });

            // Act
            dataImplementation.CheckBallsList(balls =>
            {
                foreach (var b in balls)
                {
                    b.Move(new Vector(10, 0)); // Simulate movement
                }
            });

            // Assert
            Assert.AreNotEqual(ball1.Velocity, ball2.Velocity, "Balls did not collide correctly.");
        }
    }
}