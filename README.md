# Mouse and Keyboard activity monitor

This library attaches to windows global hooks, tracks keyboard and mouse clicks and movement and raises common .NET events with KeyEventArgs and MouseEventArgs.

```csharp
private readonly KeyboardHookListener _inputListener;
private readonly KeyboardHookListener _globalKeyboardListener;

public void Subscribe()
{
    // for the application hook
    _inputListener = new KeyboardHookListener(new AppHooker());
    _inputListener.KeyDown += OnKeyDown;
    _inputListener.Start();

    // for the global hook
    _globalKeyboardListener = new KeyboardHookListener(new GlobalHooker());
    _globalKeyboardListener.KeyDown += OnGlobalKeyDown;
    _globalKeyboardListener.Start();
}

private void OnKeyDown(object sender, KeyEventArgs e)
{
    if (e.KeyCode == Keys.Menu || e.KeyCode == Keys.Alt || e.Alt)
        e.Handled = true;

    else if (e.Control == false && e.Shift == false)
    {
        switch (e.KeyCode)
        {
            case Keys.F11:
                ViewModel?.FullscreenCommand?.Execute(null);
                break;
            case Keys.Escape:
                ViewModel?.NormalScreenCommand?.Execute(null);
                break;
        }
    }
}

private void OnGlobalKeyDown(object sender, KeyEventArgs e)
{
    if (e.KeyData == Keys.PrintScreen)
    {
        // do something...   
    }
}

public void Unsubscribe()
{
    _inputListener.KeyDown -= OnKeyDown;
   _globalKeyboardListener.KeyDown -= OnGlobalKeyDown;

    //It is recommened to dispose it
    _inputListener.Dispose();
    _globalKeyboardListener.Dispose();
}
```