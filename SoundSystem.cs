using NAudio.Wave;
using NAudio.Vorbis; // для .ogg
using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov
{
    internal static class SoundSystem
    {
        private static WaveOutEvent _outputDevice;
        private static WaveStream _audioStream; // базовый класс для VorbisWaveReader, Mp3FileReader и т.д.
        private static bool _isInitialized;

        public static void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }

        public static void PlayBackgroundMusic(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            {
                Console.WriteLine($"Файл не найден: {fullPath}");
                return;
            }

            StopMusic();

            try
            {
                string ext = Path.GetExtension(fullPath).ToLower();

                if (ext == ".ogg")
                    _audioStream = new VorbisWaveReader(fullPath);
                else if (ext == ".mp3")
                    _audioStream = new Mp3FileReader(fullPath);
                else if (ext == ".wav")
                    _audioStream = new WaveFileReader(fullPath);
                else
                {
                    Console.WriteLine($"Неподдерживаемый формат: {ext}");
                    return;
                }

                _outputDevice = new WaveOutEvent();
                _outputDevice.Init(_audioStream);
                _outputDevice.Play();

                // Зацикливание
                _outputDevice.PlaybackStopped += (s, e) =>
                {
                    if (_audioStream != null)
                    {
                        _audioStream.Position = 0;
                        _outputDevice?.Play();
                    }
                };

                Console.WriteLine($"Запущена музыка: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка NAudio: {ex.Message}");
            }
        }

        public static void StopMusic()
        {
            _outputDevice?.Stop();
            _outputDevice?.Dispose();
            _audioStream?.Dispose();
            _outputDevice = null;
            _audioStream = null;
        }

        public static void Pause() => _outputDevice?.Pause();
        public static void Resume() => _outputDevice?.Play();

        public static float Volume
        {
            get => _outputDevice?.Volume ?? 1f;
            set
            {
                if (_outputDevice != null)
                    _outputDevice.Volume = MathHelper.Clamp(value, 0f, 1f);
            }
        }

        public static void Shutdown()
        {
            StopMusic();
        }
    }
}