using System.Collections.Generic;

namespace ECO
{
    public class TickerManager
    {
        private readonly List<Ticker> _activeTickerList = new();
        private readonly List<Ticker> _inActiveTickerList = new();

        public Ticker GetTicker()
        {
            return AllocTicker();
        }

        public void Update()
        {
            _activeTickerList.ForEach(x => x.Tick());
        }

        private Ticker AllocTicker()
        {
            Ticker ticker;

            if (_inActiveTickerList.Count <= 0)
            {
                ticker = new Ticker();
                _activeTickerList.Add(ticker);
            }
            else
            {
                ticker = _inActiveTickerList[0];
                _inActiveTickerList.RemoveAt(0);
            }

            return ticker;
        }
    }
}