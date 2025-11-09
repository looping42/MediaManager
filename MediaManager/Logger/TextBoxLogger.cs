using MediaManager.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MediaManager.Logger
{
    public class TextBoxLogger : IUiLogger
    {
        private readonly TextBox _textBox;

        public TextBoxLogger(TextBox textBox)
        {
            _textBox = textBox;
        }

        public void Log(string message)
        {
            // Dispatcher pour s'assurer qu'on écrit sur le thread UI
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                _textBox.ScrollToEnd();
            });
        }
    }
}