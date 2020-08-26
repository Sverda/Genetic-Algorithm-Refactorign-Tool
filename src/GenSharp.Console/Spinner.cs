using System;
using System.Threading;

namespace GenSharp.Console
{
    internal class Spinner : IDisposable
    {
        private const string _sequence = @"/-\|";
        private int _counter = 0;
        private readonly int _left;
        private readonly int _top;
        private readonly int _delay;
        private bool _active;
        private readonly Thread _thread;

        public Spinner(int left, int top, int delay = 100)
        {
            _left = left;
            _top = top;
            _delay = delay;
            _thread = new Thread(Spin);
        }

        public void Start()
        {
            _active = true;
            if (!_thread.IsAlive)
                _thread.Start();
        }

        public void Stop()
        {
            _active = false;
            Draw(' ');
        }

        private void Spin()
        {
            while (_active)
            {
                Turn();
                Thread.Sleep(_delay);
            }
        }

        private void Draw(char c)
        {
            System.Console.SetCursorPosition(_left, _top);
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write(c);
        }

        private void Turn()
        {
            Draw(_sequence[++_counter % _sequence.Length]);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
