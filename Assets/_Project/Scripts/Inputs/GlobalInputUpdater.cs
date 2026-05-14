using UnityEngine;
using Reflex.Attributes;

public class GlobalInputUpdater : MonoBehaviour
{
    [Inject] private readonly IInputService _inputService;

    private void Update()
    {
        // Luôn luôn hút dữ liệu từ AI, bất kể đang ở Menu hay Game, có Pause hay không
        if (_inputService is HandTrackingInputService handInput)
        {
            handInput.Update();
        }
    }
}