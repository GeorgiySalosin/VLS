using OpenCvSharp;
using System.Diagnostics.Metrics;
using System.Text.Json.Serialization;
using VLSGame.Services;
using VLSShared.Interfaces;

namespace VLSGame.Config.GameConfig
{
    public class GameSettings
    {

        #region Mouse settings

        [JsonPropertyName("mouse_sensitivity")]
        public float MouseSensitivity { get; set; } = .005f;    // base sensitivity for out dirty method



        [JsonPropertyName("min_speed_threshold")]
        public double MinSpeedThreshold { get; set; } = 2f; 

        [JsonPropertyName("max_speed_threshold")]
        public double MaxSpeedThreshold { get; set; } = 20f;

        [JsonPropertyName("min_sensitivity_scale")]
        public double MinSensitivityScale { get; set; } = .1f;


        #endregion


        #region Utliltary parameters

        /// <summary>  The value that clamps camera lower rotation (radians). <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double ClampVRotationMin { get; set; } = .8f;


        /// <summary>  The value that clamps camera upper rotation (radians). <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double ClampVRotationMax { get; set; } = .7f;

        /// <summary>  The total range (meters) that was written into the depth map. After this distance we gonna abandon the bullet and not count the hit. <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double MaxSnipingDistance { get; set; } = 2048f;


        /// <summary>  The distance (meters) we subtract from MaxSnipingDistance to clamp the incorrect depth values. Otherwise, the sky / far landscapes might represent not max (>2000 m) but sub-max (1995) depth. <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double MaxSnipingDistanceThresold { get; set; } = 48f;     // the distance we subtract from MaxSniping distance due to depth map incorrect behavior
                                                                          // (for example, the sky pixel might be not 100% white -> w/out thresold the distance could unexpectedly become ~ 19xx meters)
        #endregion


        // ----- Camera & camera animation settings  -----


        #region Camera sway animation

        /// <summary> Total camera sway amplitude (angular degree) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double SwayAmplitude { get; set; } = 0.06;


        /// <summary> How fast the camera sways horizontally (Hz) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double SwayFrequencyX { get; set; } = 0.47793;


        /// <summary> How fast the camera sways vertically (Hz) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double SwayFrequencyY { get; set; } = 0.659;
        #endregion


        #region Camera recoil animation 


        /// <summary> the amount the camera jumps on shot (angular °) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilVerticalBase { get; set; } = 2.0;


        /// <summary> variation thresold of the amount the camera jumps on shot (angular °) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilVerticalRandom { get; set; } = 0.4;


        /// <summary> the speed the camera jumps with on shot (angular °/ s) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilVerticalRiseSpeed { get; set; } = 60.0;


        /// <summary> the speed the camera is pulled back with after shot (angular °/ s) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilVerticalRecoverySpeed { get; set; } = 10.0;


        /// <summary> how much higher will the camera (target point) be after the shot than it was before it (angular °) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilVerticalRecoverShift { get; set; } = 0.2;


        /// <summary> the random thresold of the cameras horizontal offset on shot (angular °) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilHorizontalRange { get; set; } = 0.6;


        /// <summary> the max amount of the cameras horizontal offset on shot (angular °) <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilHorizontalMaxDeg { get; set; } = 1.8;


        /// <summary> the speed of a horizontal component of recoil <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double RecoilHorizontalInterpSpeed { get; set; } = 14.0;

        #endregion


        #region Camera fov settings 

        /// <summary> The base fov (no zoom) </summary>
        public double DefaultFOV { get; set; } = 90f;       


        /// <summary>  The minimal FOV that can be set in scope <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double MinFOVScope { get; set; } = 6f;


        /// <summary>  The maximal FOV that can be set in scope <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double MaxFOVScope { get; set; } = 30f;


        /// <summary> The target zoom level achieved when entered the scope. Is set dynamically during the match. <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public double AimingFOV { get; set; } = 11.25;


        /// <summary> The step of changing fov automatically (when zooming in/out). <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public float ZoomSpeedAuto { get; set; } = 12.0f;


        /// <summary> The step of changing scope fov manually (wheel scroll). <br></br><br></br>
        /// NOT SET BY USER CONFIG.  </summary>
        public float ZoomSpeedManual { get; set; } = 2.0f; 
        #endregion



        // bobr: add comments
        // If desired, you can put it in a separate JSON.
        #region Lobby settings
        [JsonPropertyName("selected_map_ids")]
        public List<int> SelectedMapIds { get; set; } = new List<int> { 1, 2, 3 }; // Stores saved maps in Lobby

        [JsonPropertyName("selected_gamemode_type")]
        public string SelectedGameModeType { get; set; } = "Singleplayer"; // Stores selected gamemode in Lobby

        // We cannot store the IGameMode interface in JSON, so we
        // will store a string and convert it to the interface using the property
        [JsonIgnore]
        public IGameMode SelectedGameMode
        {
            get
            {
                return SelectedGameModeType switch
                {
                    "Singleplayer" => new SinglePlayerGameMode(),
                    "Multiplayer" => new MultiPlayerGameMode(),
                    _ => new SinglePlayerGameMode()
                };
            }
            set
            {
                SelectedGameModeType = value is SinglePlayerGameMode ? "Singleplayer" : "Multiplayer";
            }
        }
        #endregion
    }
}
