using System;
using System.Windows.Controls;
using System.Xml.Linq;
using VLSGame.Config;
using VLSGame.ViewModels;

namespace VLSGame.Rendering.Content2D.HUD
{
    public class HudLayer(Panel parentPanel) : TextureLayer("HUD", RenderOrder.HUD, parentPanel)
    {
        private MatchViewModel? viewModel;

        public void Initialize(MatchViewModel viewModel)
        {
            this.viewModel = viewModel;

            viewModel.CameraProperties.PropertyChanged += OnCameraPropertyChanged;
        }
        private void OnCameraPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Handle FieldOfView Change to hide/show crosshair

            if (e.PropertyName == nameof(CameraProperties.FieldOfView))
            {
                var crosshair = GetTexture("Crosshair");
                if (crosshair == null) return;

                var currentFov = viewModel?.CameraProperties.FieldOfView ?? Configuration.Instance.GameSettings.MaxFOV;

                if (currentFov < Configuration.Instance.GameSettings.MaxFOV)
                {
                    crosshair.Hide();
                }
                else
                {
                    crosshair.Show();
                }
            }
        }

        public void ShowHudElement(string name) => ShowTexture(name);
        public void HideHudElement(string name) => HideTexture(name);
        public Texture? GetHudElement(string name) => GetTexture(name);

    }
}