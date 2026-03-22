using System;
using Reflex.Core;
using UnityEngine;

public class GameSceneInstaller : MonoBehaviour, IInstaller
{
    [Header("Global Configurations")]
    [SerializeField] private GameSettings gameSettings;

    [Header("Pool Configs")]
    [SerializeField] private FruitController fruitPrefab;
    [SerializeField] private FruitData defaultFruitData;

    public void InstallBindings(ContainerBuilder builder)
    {
        // --------------------------------------------------------
        // 1. DATA MODEL (Trái tim của game)
        // --------------------------------------------------------
        // Lấy số mạng mặc định từ GameSettings, nếu quên kéo file thì mặc định là 3
        int startingLives = gameSettings != null ? gameSettings.startingLives : 3;
        GameModel gameModel = new GameModel(startingLives);
        
        builder.RegisterValue(gameModel, new Type [] { typeof(GameModel) });

        // --------------------------------------------------------
        // 2. INPUT SERVICE (Giác quan của game)
        // --------------------------------------------------------
        IInputService inputService = new ScreenInputService();
        
        builder.RegisterValue(inputService, new Type [] { typeof(IInputService) });

        // --------------------------------------------------------
        // 3. FACTORY / POOL SERVICE (Nhà máy sản xuất)
        // --------------------------------------------------------
        // Khởi tạo nhà máy trái cây, cấp cho nó bản vẽ (Prefab, Data) và quyền truy cập Model
        FruitPoolService fruitPoolService = new FruitPoolService(fruitPrefab, defaultFruitData, gameModel);
        
        builder.RegisterValue(fruitPoolService, new Type [] { typeof(FruitPoolService) });

        // Log ra Console để đảm bảo mọi thứ khởi tạo thành công
        Debug.Log("<color=cyan>[Reflex] Đã Bind xong data!</color>");
    }
}