using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VLSGame.Rendering.Content2D;
using VLSGame.Rendering.Content3D;

namespace VLSGame.Rendering
{
    public sealed class Renderer2D
    {
        public static Renderer2D Instance { get; } = new();
        private Panel? panel;
        private readonly List<CustomObject2D> objects = new();
        private readonly Dictionary<CustomObject2D, Image> uiMap = new();
        private readonly Dictionary<CustomObject2D, List<ImageSource>> animationFrames = new();
        private readonly Dictionary<CustomObject2D, Action> animationCallbacks = new();
        private bool isInitialized = false;

        private Renderer2D() { }

        public void Initialize(Panel panel)
        {
            if (isInitialized) return;
            this.panel = panel;
            isInitialized = true;
        }

        public void AddObject(CustomObject2D obj, List<ImageSource>? frames = null, Action? onComplete = null)
        {
            if (obj == null || panel == null) return;
            if (objects.Contains(obj)) return;

            if (frames != null)
                animationFrames[obj] = frames;
            if (onComplete != null)
                animationCallbacks[obj] = onComplete;

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
                animationFrames.Remove(obj);
                animationCallbacks.Remove(obj);
            }
        }

        public CustomObject2D? GetObject(Guid id) => objects.FirstOrDefault(o => o.Id == id);
        public CustomObject2D? GetObject(string tag) => objects.FirstOrDefault(o => o.Tag == tag);

        public void Render()
        {
            if (panel == null) return;

            foreach (var kvp in uiMap)
            {
                var obj = kvp.Key;
                var img = kvp.Value;

                // Анимация (как в 3D)
                if (obj.Animation.IsPlaying && animationFrames.TryGetValue(obj, out var frames))
                {
                    int step = obj.Animation.IsReversed ? -1 : 1;
                    int newFrame = (obj.Animation.CurrentFrame ?? 0) + step;
                    if (newFrame < 0 || newFrame >= frames.Count)
                    {
                        obj.Animation.Stop();
                        obj.Animation.CurrentFrame = obj.Animation.IsReversed ? 0 : frames.Count - 1;
                        if (animationCallbacks.TryGetValue(obj, out var callback))
                            callback?.Invoke();
                    }
                    else
                    {
                        obj.Animation.CurrentFrame = newFrame;
                        obj.Texture = frames[newFrame];
                        img.Source = obj.Texture;
                        // Если панель Canvas – перецентрируем (размер мог измениться)
                        if (panel is Canvas canvas)
                            CenterImage(img, obj, canvas);
                    }
                }

                img.Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                UpdateImageTransform(img, obj);
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
            animationFrames.Clear();
            animationCallbacks.Clear();
        }
    }
}