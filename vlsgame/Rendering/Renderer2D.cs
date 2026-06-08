using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VLSGame.Config.GameConfig;
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
        private RifleState? rifleState;
        private CameraProperties? cameraProperties;


        private Renderer2D() { }

        public void Initialize(Panel panel, RifleState rifleState, CameraProperties cameraProperties)
        {
            if (isInitialized) return;
            this.panel = panel;
            this.rifleState = rifleState;
            this.cameraProperties = cameraProperties;
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

        public CustomObject2D? GetObject(String tag) => objects.FirstOrDefault(o => o.Tag == tag);

        public void Render()
        {
            if (panel == null || rifleState == null || cameraProperties == null) return;

            var uiMapCopy = uiMap.ToList();
            foreach (var kvp in uiMapCopy)
            {
                var obj = kvp.Key;
                var img = kvp.Value;

                // Обработка анимации для оружия
                if (obj.Tag == "Weapon" && obj.Animation.IsPlaying)
                {
                    int step = obj.Animation.IsReversed ? -1 : 1;
                    int newFrame = (obj.Animation.CurrentFrame ?? 0) + step;
                    if (newFrame < 0 || newFrame >= obj.Animation.FramesCount)
                    {
                        obj.Animation.Stop();
                        obj.Animation.CurrentFrame = obj.Animation.IsReversed ? 0 : obj.Animation.FramesCount - 1;
                        obj.Texture = GetTextureForCurrentState(obj, obj.Animation.CurrentFrame ?? 0);
                        obj.OnAnimationComplete?.Invoke();
                    }
                    else
                    {
                        obj.Animation.CurrentFrame = newFrame;
                        obj.Texture = GetTextureForCurrentState(obj, newFrame);
                    }
                }
                else if (obj.Tag == "Weapon")
                {
                    obj.Texture = GetTextureForCurrentState(obj, null);
                }

                if (img.Source != obj.Texture)
                    img.Source = obj.Texture;
                img.Visibility = obj.IsVisible ? Visibility.Visible : Visibility.Collapsed;

                // === Вертикальное смещение оружия (на основе ширины экрана) ===
                if (obj.Tag == "Weapon")
                {
                    var cfg = Configuration.Instance.Settings;
                    double minAngle = -Math.PI / 2 + cfg.ClampVRotationMin;
                    double maxAngle = Math.PI / 2 - cfg.ClampVRotationMax;
                    double pitch = cameraProperties.RotationX;
                    double t = (pitch - minAngle) / (maxAngle - minAngle);
                    t = Math.Clamp(t, 0.0, 1.0);
                    double normalized = t * 2.0 - 1.0;

                    double fov = cameraProperties.FieldOfView;
                    double defaultFOV = cfg.DefaultFOV;
                    double minFOV = cfg.MinFOVScope;
                    double fovFactor = 1.0;
                    if (fov <= minFOV)
                        fovFactor = 0.0;
                    else if (fov >= defaultFOV)
                        fovFactor = 1.0;
                    else
                        fovFactor = (fov - minFOV) / (defaultFOV - minFOV);

                    double maxOffset = MatchTexturePool.ScreenWidth / 30.0;
                    obj.Y = normalized * maxOffset * fovFactor;
                }

                UpdateImageTransform(img, obj);
            }
        }



        private ImageSource GetTextureForCurrentState(CustomObject2D obj, int? frame)
        {
            if (obj.Tag != "Weapon") return obj.Texture;

            switch (rifleState.State)
            {
                case ERifleState.ZoomingIn:
                case ERifleState.ZoomingOut:
                    return texturePool.GetSVLK14SZoomTexture(frame ?? (obj.Animation.CurrentFrame ?? 0));
                case ERifleState.Reloading:
                    return texturePool.GetSVLK14SReloadTexture(frame ?? (obj.Animation.CurrentFrame ?? 0));
                case ERifleState.IdleZoom:
                    return texturePool.GetSVLK14SZoomIdleTexture();
                default: // Idle
                    return texturePool.GetSVLK14SIdleTexture();
            }
        }

        public void StartZoomInAnimation(int startFrame, Action? onComplete = null)
        {
            var weapon2D = GetObject("Weapon");
            if (weapon2D == null || rifleState == null) return;
            rifleState.State = ERifleState.ZoomingIn;
            weapon2D.Animation = new Animation(26);
            weapon2D.Animation.CurrentFrame = startFrame;
            weapon2D.Animation.IsReversed = false;
            weapon2D.OnAnimationComplete = () =>
            {
                if (rifleState.State == ERifleState.ZoomingIn)
                    rifleState.State = ERifleState.IdleZoom;
                onComplete?.Invoke();
            };
            weapon2D.Animation.PlayForward();
        }

        public void StartZoomOutAnimation(int startFrame = 25, Action? onComplete = null)
        {
            var weapon2D = GetObject("Weapon");
            if (weapon2D == null || rifleState == null) return;
            rifleState.State = ERifleState.ZoomingOut;
            weapon2D.Animation = new Animation(26);
            weapon2D.Animation.CurrentFrame = startFrame;
            weapon2D.Animation.IsReversed = true;
            weapon2D.OnAnimationComplete = () =>
            {
                if (rifleState.State == ERifleState.ZoomingOut)
                    rifleState.State = ERifleState.Idle;
                onComplete?.Invoke();
            };
            weapon2D.Animation.PlayBackward();
        }

        public void StartReloadAnimation(Action? onComplete = null)
        {
            var weapon2D = GetObject("Weapon");
            if (weapon2D == null || rifleState == null) return;
            rifleState.State = ERifleState.Reloading;

            weapon2D.Animation = new Animation(181);
            weapon2D.Animation.CurrentFrame = 0;
            weapon2D.OnAnimationComplete = () =>
            {

                rifleState.State = ERifleState.Idle;
                onComplete?.Invoke();
            };
            weapon2D.Animation.PlayForward();
        }

        public void SetOnZoomOutComplete(Action callback)
        {
            var weapon2D = GetObject("Weapon");
            if (weapon2D == null) { callback?.Invoke(); return; }
            if (weapon2D.Animation.IsPlaying && weapon2D.Animation.IsReversed && weapon2D.Animation.FramesCount == 26)
            {
                var oldCallback = weapon2D.OnAnimationComplete;
                weapon2D.OnAnimationComplete = () =>
                {
                    oldCallback?.Invoke();
                    callback?.Invoke();
                };
            }
            else
            {
                callback?.Invoke();
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