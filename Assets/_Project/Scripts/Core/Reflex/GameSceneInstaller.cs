using System;
using UnityEngine;
using Reflex.Core;

public class GameSceneInstaller : MonoBehaviour, IInstaller
{
    [Header("Game Settings & Configs")]
    [SerializeField] private GameSettings gameSettings;

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

        // 4. Khởi tạo & Đăng ký Hệ thống Nhập liệu (Input)
        // Lưu ý: Đăng ký dưới dạng Interface (IInputService) để các class khác dễ dàng Inject
        IInputService inputService = new ScreenInputService();
        builder.RegisterValue(inputService, new Type [] { typeof(IInputService) });

        // 5. Khởi tạo & Đăng ký Hệ thống Âm thanh (Audio)
        AudioService audioService = new AudioService();
        builder.RegisterValue(audioService, new Type [] { typeof(AudioService) });

        // 6. Khởi tạo & Đăng ký Kho trái cây (Fruit Pool)
        // Bơm tất cả các dịch vụ trên vào đây để nó truyền xuống từng quả trái cây
        FruitPoolService fruitPoolService = new FruitPoolService(
            gameModel, 
            comboService, 
            vfxPoolService,
            audioService
        );
        builder.RegisterValue(fruitPoolService);

        Debug.Log("<color=cyan>[Reflex] Đã Bind xong tất cả Services cho Scene!</color>");
    }
}