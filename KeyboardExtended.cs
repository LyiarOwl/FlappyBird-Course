using Microsoft.Xna.Framework.Input;

namespace FlappyBirdCourse;

public static class KeyboardExtended
{
    private static KeyboardState _prevKbState;
    private static KeyboardState _currKbState;

    public static void Update()
    {
        _prevKbState = _currKbState;
        _currKbState = Keyboard.GetState();
    }

    public static bool IsKeyDown(Keys key) => _currKbState.IsKeyDown(key);
    public static bool IsKeyUp(Keys key) => _currKbState.IsKeyUp(key);
    public static bool IsKeyJustPressed(Keys key) => IsKeyDown(key) && _prevKbState.IsKeyUp(key);
    public static bool IsKeyJustReleased(Keys key) => IsKeyUp(key) && _prevKbState.IsKeyDown(key);
}