// See https://aka.ms/new-console-template for more information
using _2D_Engine_Sokov;
using _2D_Engine_Sokov.MapGeneration;
using _2D_Engine_Sokov.WarDots;

try
{
    var game = new WarDotsGame();
    game.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[FATAL] Критическая ошибка при запуске: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Console.ResetColor();
    Console.WriteLine("\nНажмите любую клавишу для выхода...");
    Console.ReadKey();
}

