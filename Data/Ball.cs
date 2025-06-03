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
using System.Drawing;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
            Color = "Red";
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;
        event EventHandler<String> NewColorNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;
        private string _color = "Blue";
        public string Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    NewColorNotification?.Invoke(this, Color);
                }
            }
        }
        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }

        #endregion private

        #region Gets/Sets
        public IVector GetPosition()
        {
            return Position;
        }
        public string changeColor()
        {
            if (this.Color == "Blue")
                this.Color = "Red";
            else if (this.Color == "Red")
                this.Color = "Yellow";
            else
                this.Color = "Blue";
            return this.Color;
        }
        public void SetPosition(IVector position)
        {
            Position = new Vector(position.x, position.y);
        }

        void IBall.Move(IVector vector)
        {
            Move(vector);
        }

        public void Move(IVector vector)
        {
            Position = new Vector(Position.x + vector.x, Position.y + vector.y);
            RaiseNewPositionChangeNotification();
        }
        #endregion Gets/Sets
    }
}
