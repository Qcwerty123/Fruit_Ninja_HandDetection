using System;
using UnityEngine;
using Reflex.Core;

public class GameSceneInstaller : MonoBehaviour, IInstaller
{
    [Header("Game Settings & Configs")]
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private AudioService audioService;
    [SerializeField] private ScreenFlashService screenFlashService;

    [Header("UDP Receiver (Cho Hand Tracking)")]
    [SerializeField] private UDPReceiverService udpReceiverService;

    public void InstallBindings(ContainerBuilder builder)
    {
        // 1. Khởi tạo & Đăng ký Lõi Dữ liệu (GameModel & GameSettings)
        int startingLives = gameSettings != null ? gameSettings.StartingLives : 3;
        builder.RegisterValue(gameSettings, new Type [] { typeof(GameSettings) });

        GameModel gameModel = new GameModel(startingLives);
        builder.RegisterValue(gameModel, new Type [] { typeof(GameModel) });

        // 2. Khởi tạo & Đăng ký Hệ thống Combo (Epic 5)
        ComboService comboService = new ComboService(gameModel);
        builder.RegisterValue(comboService, new Type [] { typeof(ComboService) });

        // 3. Khởi tạo & Đăng ký Kho hiệu ứng VFX (Epic 6 - Zero Allocation)
        VFXPoolService vfxPoolService = new VFXPoolService();
        builder.RegisterValue(vfxPoolService, new Type [] { typeof(VFXPoolService) });

        // 3.5. Khởi tạo & Đăng ký Kho mảnh vỡ (Fragment Pool) cho Dynamic Fruit
        FragmentPoolService fragmentPoolService = new FragmentPoolService();
        builder.RegisterValue(fragmentPoolService, new Type [] { typeof(FragmentPoolService) });

        // 4. Khởi tạo & Đăng ký Hệ thống Nhập liệu (Input)
        ScreenInputService screenInputService = new ScreenInputService();
        builder.RegisterValue(screenInputService, new Type [] { typeof(ScreenInputService) });

        HandTrackingInputService handTrackingInputService = new HandTrackingInputService(udpReceiverService);
        builder.RegisterValue(handTrackingInputService, new Type [] { typeof(HandTrackingInputService) });

        IInputService inputRouter = new InputRouter(gameModel, screenInputService, handTrackingInputService);
        builder.RegisterValue(inputRouter, new Type [] { typeof(IInputService) });

        // 5. Khởi tạo & Đăng ký Hệ thống Âm thanh (Audio)
        builder.RegisterValue(audioService, new Type [] { typeof(AudioService) });

        // 6. Khởi tạo & Đăng ký Hệ thống Flash màn hình (Screen Flash)
        builder.RegisterValue(screenFlashService);

        // 7. Khởi tạo & Đăng ký Kho trái cây (Fruit Pool)
        // Bơm tất cả các dịch vụ trên vào đây để nó truyền xuống từng quả trái cây
        FruitPoolService fruitPoolService = new FruitPoolService(
            gameModel, 
            comboService, 
            vfxPoolService,
            audioService,
            screenFlashService,
            fragmentPoolService
        );
        builder.RegisterValue(fruitPoolService);

        Debug.Log("<color=cyan>[Reflex] Đã Bind xong tất cả Services cho Scene!</color>");
    }
}