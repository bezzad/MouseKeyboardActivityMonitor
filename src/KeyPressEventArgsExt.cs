﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor.WinApi;

namespace MouseKeyboardActivityMonitor
{
    ///<summary>
    /// Provides extended data for the <see cref='KeyboardHookListener.KeyPress'/> event.
    ///</summary>
    public class KeyPressEventArgsExt : KeyPressEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref='KeyPressEventArgsExt'/> class.
        /// </summary>
        /// <param name="keyChar">Character corresponding to the key pressed. 0 char if represens a system or functional non char key.</param>
        public KeyPressEventArgsExt(char keyChar)
            : base(keyChar)
        {
            IsNonChar = false;
            Timestamp = Environment.TickCount;
        }

        private static KeyPressEventArgsExt CreateNonChar()
        {
            var e = new KeyPressEventArgsExt((char)0x0);
            e.IsNonChar = true;
            e.Timestamp = Environment.TickCount;
            return e;
        }

        /// <summary>
        /// Creates <see cref="KeyPressEventArgsExt"/> from Windows Message parameters.
        /// </summary>
        /// <param name="wParam">The first Windows Message parameter.</param>
        /// <param name="lParam">The second Windows Message parameter.</param>
        /// <param name="isGlobal">Specifies if the hook is local or global.</param>
        /// <returns>A new KeyPressEventArgsExt object.</returns>
        internal static KeyPressEventArgsExt FromRawData(int wParam, IntPtr lParam, bool isGlobal)
        {
            return isGlobal ?
                FromRawDataGlobal(wParam, lParam) :
                FromRawDataApp(wParam, lParam);
        }

        /// <summary>
        /// Creates <see cref="KeyPressEventArgsExt"/> from Windows Message parameters,
        /// based upon a local application hook.
        /// </summary>
        /// <param name="wParam">The first Windows Message parameter.</param>
        /// <param name="lParam">The second Windows Message parameter.</param>
        /// <returns>A new KeyPressEventArgsExt object.</returns>
        private static KeyPressEventArgsExt FromRawDataApp(int wParam, IntPtr lParam)
        {
            //http://msdn.microsoft.com/en-us/library/ms644984(v=VS.85).aspx

            const uint maskKeydown = 0x40000000;         // for bit 30
            const uint maskKeyup = 0x80000000;           // for bit 31
            const uint maskScanCode = 0xff0000;          // for bit 23-16

            var flags = 0u;
#if IS_X64
            // both of these are ugly hacks. Is there a better way to convert a 64bit IntPtr to uint?

            // flags = uint.Parse(lParam.ToString());
            flags = Convert.ToUInt32(lParam.ToInt64());
#else
            flags = (uint)lParam;
#endif

            //bit 30 Specifies the previous key state. The value is 1 if the key is down before the message is sent; it is 0 if the key is up.
            var wasKeyDown = (flags & maskKeydown) > 0;
            //bit 31 Specifies the transition state. The value is 0 if the key is being pressed and 1 if it is being released.
            var isKeyReleased = (flags & maskKeyup) > 0;

            if (!wasKeyDown && !isKeyReleased)
            {
                return CreateNonChar();
            }

            var virtualKeyCode = wParam;
            var scanCode = checked((int)(flags & maskScanCode));
            const int fuState = 0;

            char ch;
            var isSuccessfull = Keyboard.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out ch);
            if (!isSuccessfull)
            {
                return CreateNonChar();
            }
            
            return new KeyPressEventArgsExt(ch);

        }

        /// <summary>
        /// Creates <see cref="KeyPressEventArgsExt"/> from Windows Message parameters,
        /// based upon a system-wide hook.
        /// </summary>
        /// <param name="wParam">The first Windows Message parameter.</param>
        /// <param name="lParam">The second Windows Message parameter.</param>
        /// <returns>A new KeyPressEventArgsExt object.</returns>
        internal static KeyPressEventArgsExt FromRawDataGlobal(int wParam, IntPtr lParam)
        {
            if (wParam != Messages.WM_KEYDOWN)
            {
                return CreateNonChar();
            }

            var keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));

            var virtualKeyCode = keyboardHookStruct.VirtualKeyCode;
            var scanCode = keyboardHookStruct.ScanCode;
            var fuState = keyboardHookStruct.Flags;

            char ch;
            var isSuccessfull = Keyboard.TryGetCharFromKeyboardState(virtualKeyCode, scanCode, fuState, out ch);
            if (!isSuccessfull)
            {
                return CreateNonChar();
            }

            var e = new KeyPressEventArgsExt(ch);
            e.Timestamp = keyboardHookStruct.Time;		// Update the timestamp to use the actual one from KeyboardHookStruct

            return e;
        }

        /// <summary>
        /// True if represents a system or functional non char key.
        /// </summary>
        public bool IsNonChar { get; private set; }
        
        /// <summary>
        /// The system tick count of when the event occured.
        /// </summary> 
        public int Timestamp { get; private set; }
    }
}
