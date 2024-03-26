using System;
using System.Windows.Forms;

namespace Azil_maGUIc
{
    public static class ScoreValidator
    {
        public ScoreValidator()
        {

        }

        public static void digitValidator(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsWhiteSpace(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}