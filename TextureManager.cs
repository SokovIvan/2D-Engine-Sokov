using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public static class TextureManager
    {
        private static readonly Dictionary<string, TextureEntry> _textures = new Dictionary<string, TextureEntry>();
        private static readonly ConcurrentQueue<TextureLoadRequest> _loadQueue = new ConcurrentQueue<TextureLoadRequest>();
        private static GraphicsDevice _graphicsDevice;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public static void Update()
        {
            // Обработка очереди загрузки текстур
            while (_loadQueue.TryDequeue(out var request))
            {
                try
                {
                    if (_textures.TryGetValue(request.Path, out var entry))
                    {
                        // Текстура уже загружена, просто увеличиваем счетчик
                        entry.ReferenceCount++;
                        request.SetTexture(entry.Texture);
                    }
                    else
                    {
                        // Загружаем новую текстуру
                        using var stream = File.OpenRead(request.Path);
                        var texture = Texture2D.FromStream(_graphicsDevice, stream);

                        var newEntry = new TextureEntry
                        {
                            Texture = texture,
                            ReferenceCount = 1,
                            Path = request.Path
                        };

                        _textures[request.Path] = newEntry;
                        request.SetTexture(texture);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading texture '{request.Path}': {ex.Message}");
                    request.SetTexture(null);
                }
            }

            // Проверка на неиспользуемые текстуры
            var unusedTextures = _textures.Where(pair => pair.Value.ReferenceCount <= 0).ToList();
            foreach (var unused in unusedTextures)
            {
                unused.Value.Texture.Dispose();
                _textures.Remove(unused.Key);
            }
        }

        public static void LoadTexture(object requester, string path, Action<Texture2D> callback)
        {
            _loadQueue.Enqueue(new TextureLoadRequest
            {
                Requester = requester,
                Path = path,
                Callback = callback
            });
        }

        public static void ReleaseTexture(string path)
        {
            if (_textures.TryGetValue(path, out var entry))
            {
                entry.ReferenceCount--;
            }
        }

        private class TextureLoadRequest
        {
            public object Requester { get; set; }
            public string Path { get; set; }
            public Action<Texture2D> Callback { get; set; }

            public void SetTexture(Texture2D texture)
            {
                Callback?.Invoke(texture);
            }
        }

        private class TextureEntry
        {
            public Texture2D Texture { get; set; }
            public int ReferenceCount { get; set; }
            public string Path { get; set; }
        }
    }
}
