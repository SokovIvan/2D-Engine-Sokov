// See https://aka.ms/new-console-template for more information
using _2D_Engine_Sokov;
using _2D_Engine_Sokov.MapGeneration;

// Game game = new Game();
// game.Run();
// while (game._isRunning) { 
//}

Console.WriteLine("1. Генерирую новую карту (96x96)...");

MapState originalMap = MapGenerator.GenerateMapState(width: 150, height: 150, hash: 41);

Console.WriteLine($"   Карта сгенерирована.");
Console.WriteLine($"   Путь к сырым данным: {originalMap.path_to_image}");

Console.WriteLine($"   Пример точки [0,0]: Тип={originalMap.getGroundState(0, 0)}, Высота={originalMap.getHeight(0, 0)}");
Console.WriteLine($"   Пример точки [48,48]: Тип={originalMap.getGroundState(48, 48)}, Высота={originalMap.getHeight(48, 48)}");

Console.WriteLine("\n2. Теперь попробую загрузить эту же карту из файла...");

MapState loadedMap = MapReader.LoadFromImage(originalMap.path_to_image);

if (loadedMap != null)
{
    Console.WriteLine("   Карта успешно загружена!");
}