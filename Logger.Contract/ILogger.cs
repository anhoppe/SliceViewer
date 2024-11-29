#region

using System;

#endregion

namespace Logger.Contract
{
    public interface ILogger
    {
        #region Public Methods

        void Error(string message, Exception exception = null);

        void Info(string message, Exception exception = null);

        void Warn(string message, Exception exception = null);

        #endregion
    }
}
