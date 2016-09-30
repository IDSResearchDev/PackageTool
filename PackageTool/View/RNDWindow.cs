using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace PackageTool.View
{
  
    public class RNDWindow : Window
    {
       
        /// <summary>
        /// Apply "inactive" effect on window
        /// </summary>
        /// <param name="win"></param>
        private void ApplyEffect(Window win)
        {
            var effect = new BlurEffect { Radius = 2 };
            win.Effect = effect;
        }

        /// <summary>
        /// Remove "inactive" effects
        /// </summary>
        /// <param name="win"></param>
        private void ClearEffect(Window win)
        {
            win.Effect = null;
        }

        /// <summary>
        /// Show dialog and blur owner
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public bool? ShowDialog(Window owner)
        {
            Owner = owner;
          
            var overrideCursor = Mouse.OverrideCursor;

            try
            {
                Mouse.OverrideCursor = null;
                ApplyEffect(owner);
                return ShowDialog();
            }
            finally
            {
                Owner = null;
                ClearEffect(owner);
                Mouse.OverrideCursor = overrideCursor;
            }
        }
    }
}