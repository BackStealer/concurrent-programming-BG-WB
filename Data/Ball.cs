//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;

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
