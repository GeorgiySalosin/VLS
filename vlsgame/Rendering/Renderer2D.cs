using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VLSGame.Rendering.Content2D;

namespace VLSGame.Rendering
{
    public sealed class Renderer2D
    {
        public static Renderer2D Instance { get; } = new();
        private Panel? panel;
        private readonly List<CustomObject2D> objects = new();
        private readonly Dictionary<CustomObject2D, Image> uiMap = new();
        private bool isInitialized = false;

        private Renderer2D() { }

        public void Initialize(Panel panel)
        {
            if (isInitialized) return;
            this.panel = panel;
            isInitialized = true;
        }

        public void AddObject(CustomObject2D obj)
        {
            if (obj == null || panel == null) return;
            if (objects.Contains(obj)) return;

            // Создаём Image и сразу добавляем в Panel
            var img = new Image
            {
                Source = obj.Texture,
                Stretch = Stretch.None,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed
            };

            CenterImage(img, obj, panel as Canvas);

            panel.Children.Add(img);

            objects.Add(obj);
            uiMap[obj] = img;
        }
        private void CenterImage(Image img, CustomObject2D obj, Canvas canvas)
        {
            // Получаем фактические размеры текстуры
            double width = obj.Texture.Width;
            double height = obj.Texture.Height;


            double centerX = canvas.ActualWidth / 2;
            double centerY = canvas.ActualHeight / 2;

            double left = centerX - width / 2;
            double top = centerY - height / 2;

            Canvas.SetLeft(img, left);
            Canvas.SetTop(img, top);
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

        public void RemoveObject(CustomObject2D obj)
        {
            if (obj != null && uiMap.TryGetValue(obj, out var img))
            {
                panel?.Children.Remove(img);
                uiMap.Remove(obj);
                objects.Remove(obj);
            }
        }

        public CustomObject2D? GetObject(Guid id) => objects.FirstOrDefault(o => o.Id == id);
        public CustomObject2D? GetObject(string tag) => objects.FirstOrDefault(o => o.Tag == tag);

        public void Render()
        {
            if (panel == null) return;

            // Обновляем видимость и трансформации у всех существующих Image
            foreach (var kvp in uiMap)
            {
                var obj = kvp.Key;
                var img = kvp.Value;

                img.Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                UpdateImageTransform(img, obj);
            }
        }

        private static void UpdateImageTransform(Image img, CustomObject2D obj)
        {
            if (Math.Abs(obj.Scale - 1.0) < 0.001 && obj.X == 0 && obj.Y == 0)
            {
                img.RenderTransform = null;
            }
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