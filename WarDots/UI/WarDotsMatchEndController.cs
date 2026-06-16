using _2D_Engine_Sokov.UIElements;
using System;
using System.Collections.Generic;

namespace _2D_Engine_Sokov.WarDots.UI
{
    internal class WarDotsMatchEndController : UIElement
    {
        // Пути к уровням, задаются прямо в XML
        public string NextLevelPath { get; set; } = string.Empty;
        public string CurrentLevelPath { get; set; } = string.Empty;

        // Задержка перед переходом (секунды), чтобы анимации и звуки успели проиграться
        public double LoadDelay { get; set; } = 2.5;

        private bool _matchEnded = false;
        private bool _playerVictory = false;
        private double _delayTimer = 0.0;

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            // Если игра уже загружается или исход определён — просто ждём задержку
            if (WarDotsGame.Instance.IsLoading || _matchEnded)
            {
                if (_matchEnded)
                {
                    _delayTimer += deltaTime;
                    if (_delayTimer >= LoadDelay)
                    {
                        string target = _playerVictory ? NextLevelPath : CurrentLevelPath;
                        if (!string.IsNullOrWhiteSpace(target))
                            WarDotsGame.Instance.LoadLevel(target);
                    }
                }
                return;
            }

            // Проверяем статус баз на карте
            bool playerBaseAlive = false;
            bool enemyBaseAlive = false;

            // Безопасный снимок коллекции из GameContext (защита от многопоточности)
            var snapshot = GameContext.GetGameObjects();
            foreach (var obj in snapshot)
            {
                if (!obj.IsActive) continue;

                string typeName = obj.GetType().Name;
                if (typeName == "WarDotsPlayerBase") playerBaseAlive = true;
                if (typeName == "WarDotsEnemyBase") enemyBaseAlive = true;

                if (playerBaseAlive && enemyBaseAlive) break;
            }

            // Определяем исход матча
            if (!playerBaseAlive)
            {
                _matchEnded = true;
                _playerVictory = false; // Поражение
            }
            else if (!enemyBaseAlive)
            {
                _matchEnded = true;
                _playerVictory = true;  // Победа
            }
        }
    }
}