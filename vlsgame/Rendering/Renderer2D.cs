using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VLSGame.Models;
using VLSGame.Rendering.Content2D;
using VLSGame.ViewModels;

namespace VLSGame.Rendering
{
    public sealed class Renderer2D
    {
        public static Renderer2D Instance { get; } = new();
        private Panel? panel;
        private readonly List<CustomObject2D> objects = new();
        private readonly Dictionary<CustomObject2D, Image> uiMap = new();
        private bool isInitialized = false;
        private readonly MatchTexturePool texturePool = MatchTexturePool.Instance;
        private RifleState? rifleState; // добавляем ссылку на состояние винтовки

        private Renderer2D() { }

        public void Initialize(Panel panel, RifleState rifleState)
        {
            if (isInitialized) return;
            this.panel = panel;
            this.rifleState = rifleState;
            isInitialized = true;
        }

        public void AddObject(CustomObject2D obj)
        {
            if (obj == null || panel == null) return;
            if (objects.Contains(obj)) return;

            var img = new Image
            {
                Source = obj.Texture,
                Stretch = Stretch.None,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed
            };
            panel.Children.Add(img);
            objects.Add(obj);
            uiMap[obj] = img;

            if (panel is Canvas canvas)
                CenterImage(img, obj, canvas);
        }

        private void CenterImage(Image img, CustomObject2D obj, Canvas canvas)
        {
            double width = obj.Texture.Width;
            double height = obj.Texture.Height;
            if (double.IsNaN(width) || double.IsNaN(height) || width <= 0 || height <= 0)
            {
                img.Loaded += (s, e) => CenterImage(img, obj, canvas);
                return;
            }
            Canvas.SetLeft(img, (canvas.ActualWidth / 2) - (width / 2));
            Canvas.SetTop(img, (canvas.ActualHeight / 2) - (height / 2));
        }

        public void RemoveObject(Guid id)
        {
            var obj = objects.FirstOrDefault(o => o.Id == id);
            if (obj != null && uiMap.TryGetValue(obj, out var img))
            {
                panel?.Children.Remove(img);
                uiMap.Remove(obj);
                objects.Remove(obj);
            }
        }

        public CustomObject2D? GetObject(Guid id) => objects.FirstOrDefault(o => o.Id == id);

        public void Render()
        {
            if (panel == null || rifleState == null) return;

            // Создаём копию, чтобы избежать модификации коллекции во время итерации
            var uiMapCopy = uiMap.ToArray();

            foreach (var kvp in uiMapCopy)
            {
                var obj = kvp.Key;
                // Проверяем, не был ли объект удалён за время итерации
                if (!uiMap.ContainsKey(obj)) continue;
                var img = kvp.Value;

                // Обновляем текстуру для Scope в зависимости от состояния винтовки
                if (obj.Tag == "Scope")
                {
                    switch (rifleState.State)
                    {
                        case ERifleState.Idle:
                            obj.Texture = texturePool.GetSVLK14SIdleTexture();
                            break;
                        case ERifleState.ZoomingIn:
                            // анимация уже управляется через Animation
                            break;
                        case ERifleState.ZoomingOut:
                            break;
                        case ERifleState.IdleZoom:
                            obj.Texture = texturePool.GetSVLK14SZoomIdleTexture();
                            break;
                        case ERifleState.Reloading:
                            // Scope может быть скрыт или оставлен последним кадром
                            break;
                    }
                }

                // Обработка активной анимации (для Scope и Reload)
                if (obj.Animation.IsPlaying)
                {
                    int step = obj.Animation.IsReversed ? -1 : 1;
                    int newFrame = (obj.Animation.CurrentFrame ?? 0) + step;
                    if (newFrame < 0 || newFrame >= obj.Animation.FramesCount)
                    {
                        obj.Animation.Stop();
                        obj.Animation.CurrentFrame = obj.Animation.IsReversed ? 0 : obj.Animation.FramesCount - 1;
                        obj.OnAnimationComplete?.Invoke();
                    }
                    else
                    {
                        obj.Animation.CurrentFrame = newFrame;
                        // Получаем текстуру из пула по типу анимации
                        if (obj.Tag == "Scope")
                            obj.Texture = texturePool.GetSVLK14SZoomTexture(newFrame);
                        else if (obj.Tag == "Reload")
                            obj.Texture = texturePool.GetSVLK14SReloadTexture(newFrame);
                    }
                }

                // Применяем текстуру, если изменилась
                if (img.Source != obj.Texture)
                    img.Source = obj.Texture;

                img.Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                UpdateImageTransform(img, obj);

                if (panel is Canvas canvas && obj.Texture != null && obj.Texture.Width > 0)
                    CenterImage(img, obj, canvas);
            }
        }

        private static void UpdateImageTransform(Image img, CustomObject2D obj)
        {
            if (Math.Abs(obj.Scale - 1.0) < 0.001 && obj.X == 0 && obj.Y == 0)
                img.RenderTransform = null;
            else
            {
                var group = new TransformGroup();
                group.Children.Add(new ScaleTransform(obj.Scale, obj.Scale));
                group.Children.Add(new TranslateTransform(obj.X, obj.Y));
                img.RenderTransform = group;
            }
        }

        public void RemoveAll()
        {
            foreach (var img in uiMap.Values)
                panel?.Children.Remove(img);
            uiMap.Clear();
            objects.Clear();
        }
    }
}