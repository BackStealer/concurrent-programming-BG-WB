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
using System.Reflection;
using System.Threading;
using TP.ConcurrentProgramming.Data;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor
    private CancellationTokenSource _cancellationTokenSource = new();
    private List<Ball> balls = new();
    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

        #endregion ctor

        #region BusinessLogicAbstractAPI
        public override void Dispose()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            }

            Debug.WriteLine("BusinessLogicImplementation.Dispose() - Start");

            try
            {
                Debug.WriteLine("BusinessLogicImplementation.Dispose() - Disposing layerBellow...");
                layerBellow.Dispose();
                Debug.WriteLine("BusinessLogicImplementation.Dispose() - layerBellow disposed.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BusinessLogicImplementation.Dispose() - Exception: {ex.Message}");
                throw;
            }
            _colorChangeTimer?.Stop();
            _colorChangeTimer?.Dispose();
            layerBellow.Dispose();
            Disposed = true;
            Debug.WriteLine("BusinessLogicImplementation.Dispose() - End");
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, databall) =>
            {
                var ball = new Ball(databall);
                balls.Add(ball); // Dodaj piłkę do listy
                upperLayerHandler(new Position(startingPosition.x, startingPosition.x), ball);
            });
            StartChangingColor();

        }

        #endregion BusinessLogicAbstractAPI
        private System.Timers.Timer? _colorChangeTimer;
        private void StartChangingColor()
        {
            _colorChangeTimer = new System.Timers.Timer(3000);
            _colorChangeTimer.Elapsed += (sender, args) =>
            {
                foreach (Ball ball in balls)
                {
                    ball.changeDataBallColor();
                }
            };
            _colorChangeTimer.AutoReset = true;
            _colorChangeTimer.Start();
        }

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}