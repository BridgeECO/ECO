using System;

namespace ECO
{
    public class Ticker
    {
        public bool IsActive { get; set; } = true;

        private int _frameInterval = 10;
        private int _curFrame;

        private Action _onTick;

        public void SetOnTickAct(Action onTick)
        {
            _onTick = onTick;
        }

        public void SetActive(bool isActive)
        {
            this.IsActive = IsActive;
        }

        public void Tick()
        {
            if (!IsActive)
                return;

            _curFrame++;
            if (_curFrame >= _frameInterval)
            {
                _curFrame = 0;
                _onTick?.Invoke();
            }
        }
    }
}