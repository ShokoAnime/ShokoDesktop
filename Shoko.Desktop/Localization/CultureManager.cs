//
//      FILE:   CultureManager.cs.
//
// COPYRIGHT:   Copyright 2008 
//              Infralution
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Reflection;
using System.Runtime.InteropServices;
namespace Infralution.Localization.Wpf
{

    /// <summary>
    /// Provides the ability to change the UICulture for WPF Windows and controls
    /// dynamically.  
    /// </summary>
    /// <remarks>
    /// XAML elements that use the <see cref="ResxExtension"/> are automatically
    /// updated when the <see cref="CultureManager.UICulture"/> property is changed.
    /// </remarks>
    public static class CultureManager
    {
        #region Static Member Variables

        /// <summary>
        /// Current UICulture of the application
        /// </summary>
        private static CultureInfo _uiCulture;

        /// <summary>
        /// Should the <see cref="Thread.CurrentCulture"/> be changed when the
        /// <see cref="UICulture"/> changes.
        /// </summary>
        private static bool _synchronizeThreadCulture = true;

        #endregion

        #region Public Interface

        /// <summary>
        /// Raised when the <see cref="UICulture"/> is changed
        /// </summary>
        /// <remarks>
        /// Since this event is static if the client object does not detach from the event a reference
        /// will be maintained to the client object preventing it from being garbage collected - thus
        /// causing a potential memory leak. 
        /// </remarks>
        public static event EventHandler UICultureChanged;

        /// <summary>
        /// Sets the UICulture for the WPF application and raises the <see cref="UICultureChanged"/>
        /// event causing any XAML elements using the <see cref="ResxExtension"/> to automatically
        /// update
        /// </summary>
        public static CultureInfo UICulture
        {
            get
            {
                if (_uiCulture == null)
                {
                    _uiCulture = Thread.CurrentThread.CurrentUICulture;
                }
                return _uiCulture;
            }
            set
            {
                if (value != UICulture)
                {
                    _uiCulture = value;
                    Thread.CurrentThread.CurrentUICulture = value;
                    if (SynchronizeThreadCulture)
                    {
                        SetThreadCulture(value);
                    }
                    UICultureExtension.UpdateAllTargets();
                    ResxExtension.UpdateAllTargets();
                    if (UICultureChanged != null)
                    {
                        UICultureChanged(null, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// If set to true then the <see cref="Thread.CurrentCulture"/> property is changed
        /// to match the current <see cref="UICulture"/>
        /// </summary>
        public static bool SynchronizeThreadCulture
        {
            get { return _synchronizeThreadCulture; }
            set
            {
                _synchronizeThreadCulture = value;
                if (value)
                {
                    SetThreadCulture(UICulture);
                }
            }
        }

        #endregion


        /// <summary>
        /// Set the thread culture to the given culture
        /// </summary>
        /// <param name="value">The culture to set</param>
        /// <remarks>If the culture is neutral then creates a specific culture</remarks>
        private static void SetThreadCulture(CultureInfo value)
        {
            if (value.IsNeutralCulture)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(value.Name);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = value;
            }
        }



    }

}
