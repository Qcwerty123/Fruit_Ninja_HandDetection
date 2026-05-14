using UnityEngine;

public class InputRouter : IInputService
{
    private readonly GameModel _gameModel;
    private readonly ScreenInputService _screenInput;
    private readonly HandTrackingInputService _handInput;

    public InputRouter(GameModel gameModel, ScreenInputService screenInput, HandTrackingInputService handInput)
    {
        _gameModel = gameModel;
        _screenInput = screenInput;
        _handInput = handInput;
    }

    public bool IsSwiping()
    {
        return _gameModel.CurrentInput.Value == InputMethod.Mouse 
            ? _screenInput.IsSwiping() 
            : _handInput.IsSwiping();
    }

    public Vector2 GetCurrentPosition()
    {
        return _gameModel.CurrentInput.Value == InputMethod.Mouse 
            ? _screenInput.GetCurrentPosition() 
            : _handInput.GetCurrentPosition();
    }
}