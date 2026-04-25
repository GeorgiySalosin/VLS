using System.Text.Json.Serialization;

namespace VLSGame.Config
{
    public class GameSettings
    {
        [JsonPropertyName("mouse_sensitivity")]
        public float MouseSensitivity { get; set; } = .005f;    // base sensitivity for out dirty method

        [JsonPropertyName("speed_buffer_size")]
        public int SpeedBufferSize { get; set; } = 5;         // Number of different speed levels

        [JsonPropertyName("min_speed_threshold")]
        public double MinSpeedThreshold { get; set; } = 2f; 

        [JsonPropertyName("max_speed_threshold")]
        public double MaxSpeedThreshold { get; set; } = 20f;

        [JsonPropertyName("min_sensitivity_scale")]
        public double MinSensitivityScale { get; set; } = .1f;

        [JsonPropertyName("clamp_vrotation_min")]
        public double ClampVRotationMin { get; set; } = .7f;    // An option to block looking too low 

        [JsonPropertyName("clamp_vrotation_max")]
        public double ClampVRotationMax { get; set; } = .7f;    // Same but blocks looking high

        [JsonPropertyName("zoom_speed")]
        public double ZoomSpeed { get; set; } = .1f;            // Currently: an amount of FOV reduced per a single mousewheel step

        [JsonPropertyName("min_fov")]
        public double MinFOV { get; set; } = 6f;        // Just a general setting that restricts zooming by a 15x

        [JsonPropertyName("max_fov")]
        public double MaxFOV { get; set; } = 90f;       // The base fov (no zoom)

        [JsonPropertyName("max_sniping_distance")]
        public double MaxSnipingDistance { get; set; } = 2048f;       // The distance up to which we can count hits (The range that was written into the depth map) - after this distance we gonna abandon the bullet

        [JsonPropertyName("max_sniping_distance_thresold")]
        public double MaxSnipingDistanceThresold { get; set; } = 48f;     // the distance we subtract from MaxSniping distance due to depth map incorrect behavior
                                                                          // (for example, the sky pixel might be not 100% white -> w/out thresold the distance could unexpectedly become ~ 19xx meters)

        [JsonPropertyName("selected_map_ids")]
        public List<int> SelectedMapIds { get; set; } = new List<int> { 1, 2, 3 }; // Stores saved maps in Lobby
    }
}
