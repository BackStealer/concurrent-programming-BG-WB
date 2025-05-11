//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
    }

    #endregion ctor

    #region public API

    public void Start(int numberOfBalls)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      ModelLayer.Start(numberOfBalls);
      Observer.Dispose();
    }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      System.Diagnostics.Debug.WriteLine("weszło do funkcji");
      if (!Disposed)
      {
        System.Diagnostics.Debug.WriteLine("krok1");
        if (disposing)
        {
                    System.Diagnostics.Debug.WriteLine("krok2");
                    Balls.Clear();
                    System.Diagnostics.Debug.WriteLine("krok3");
                    Observer.Dispose();
                    System.Diagnostics.Debug.WriteLine("krok4");
                    ModelLayer.Dispose();
                    System.Diagnostics.Debug.WriteLine("krok5");
                }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        System.Diagnostics.Debug.WriteLine("krok6");
                Disposed = true;
        System.Diagnostics.Debug.WriteLine("chyba zabiło");
      }
    }

    public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
      System.Diagnostics.Debug.WriteLine("Chyba zabija wątki");
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;

    #endregion private
  }
}